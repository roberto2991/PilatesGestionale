using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PilatesStudio.Data;
using PilatesStudio.Models;
using PilatesStudio.Models.ViewModels;

namespace PilatesStudio.Controllers;

/// <summary>
/// Gestione delle singole occorrenze (sessioni datate) di un corso e registrazione presenze.
/// Accessibile a Admin e Staff (istruttrici operanti come Staff).
/// </summary>
[Authorize(Roles = "Admin,Staff")]
public class OccorrenzeController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<OccorrenzeController> _logger;

    public OccorrenzeController(ApplicationDbContext db, ILogger<OccorrenzeController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ─────────────────────── INDEX (calendario sessioni del corso) ───────────────────────

    [HttpGet]
    public async Task<IActionResult> Index(int corsoId, StatoOccorrenza? stato, bool soloFuture = false)
    {
        var corso = await _db.TipologieCorsi.FirstOrDefaultAsync(c => c.Id == corsoId);
        if (corso is null) return NotFound();

        var query = _db.OccorrenzeCorso
            .Where(o => o.TipologiaCorsoId == corsoId);

        if (stato is not null)
            query = query.Where(o => o.Stato == stato);

        if (soloFuture)
        {
            var oggi = DateTime.Now.Date;
            query = query.Where(o => o.Data >= oggi);
        }

        var righe = await query
            .OrderBy(o => o.Data).ThenBy(o => o.OraInizio)
            .Select(o => new OccorrenzaRigaViewModel
            {
                Occorrenza = o,
                NumPresenzeRegistrate = o.Presenze.Count,
                NumPresenti = o.Presenze.Count(p => p.Presente)
            })
            .ToListAsync();

        var numIscritti = await _db.IscrizioniCorso.CountAsync(i => i.TipologiaCorsoId == corsoId);

        return View(new OccorrenzeListViewModel
        {
            Corso = corso,
            Occorrenze = righe,
            NumeroIscritti = numIscritti,
            Stato = stato,
            SoloFuture = soloFuture
        });
    }

    // ─────────────────────── DETTAGLIO + REGISTRAZIONE PRESENZE ───────────────────────

    [HttpGet]
    public async Task<IActionResult> Dettaglio(int id)
    {
        var occorrenza = await _db.OccorrenzeCorso
            .Include(o => o.TipologiaCorso)
            .Include(o => o.Presenze)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (occorrenza is null) return NotFound();

        var vm = await BuildDettaglioAsync(occorrenza);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvaPresenze(int id, List<int> presenti)
    {
        var occorrenza = await _db.OccorrenzeCorso
            .Include(o => o.Presenze)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (occorrenza is null) return NotFound();

        if (occorrenza.Annullata)
        {
            TempData["Error"] = "Impossibile registrare presenze su una sessione annullata.";
            return RedirectToAction(nameof(Dettaglio), new { id });
        }

        // Clienti attualmente iscritti al corso
        var iscritti = await _db.IscrizioniCorso
            .Where(i => i.TipologiaCorsoId == occorrenza.TipologiaCorsoId)
            .Select(i => i.ClienteId)
            .ToListAsync();

        var presentiSet = presenti.ToHashSet();
        var presenzeEsistenti = occorrenza.Presenze.ToDictionary(p => p.ClienteId);
        var operatore = User.Identity?.Name;
        var adesso = DateTime.Now;

        foreach (var clienteId in iscritti)
        {
            var presente = presentiSet.Contains(clienteId);

            if (presenzeEsistenti.TryGetValue(clienteId, out var presenza))
            {
                // Aggiorna lo stato (le presenze non vengono mai cancellate, solo aggiornate)
                presenza.Presente = presente;
                presenza.DataRegistrazione = adesso;
                presenza.RegistrataDa = operatore;
            }
            else
            {
                _db.PresenzeCorso.Add(new PresenzaCorso
                {
                    OccorrenzaCorsoId = occorrenza.Id,
                    ClienteId = clienteId,
                    Presente = presente,
                    DataRegistrazione = adesso,
                    RegistrataDa = operatore
                });
            }
        }

        // Segna la sessione come svolta una volta registrate le presenze
        if (occorrenza.Stato == StatoOccorrenza.Programmata)
            occorrenza.Stato = StatoOccorrenza.Svolta;

        await _db.SaveChangesAsync();

        var numPresenti = iscritti.Count(presentiSet.Contains);
        TempData["Success"] = $"Presenze registrate: {numPresenti} presenti su {iscritti.Count} iscritti.";
        return RedirectToAction(nameof(Dettaglio), new { id });
    }

    // ─────────────────────── MODIFICA SINGOLA OCCORRENZA ───────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Modifica(OccorrenzaEditInputModel model)
    {
        var occorrenza = await _db.OccorrenzeCorso.FindAsync(model.Id);
        if (occorrenza is null) return NotFound();

        if (!TimeSpan.TryParse(model.OraInizio, out var inizio) ||
            !TimeSpan.TryParse(model.OraFine, out var fine))
        {
            TempData["Error"] = "Orari non validi.";
            return RedirectToAction(nameof(Dettaglio), new { id = model.Id });
        }

        if (fine <= inizio)
        {
            TempData["Error"] = "L'ora di fine deve essere successiva all'ora di inizio.";
            return RedirectToAction(nameof(Dettaglio), new { id = model.Id });
        }

        occorrenza.Data = model.Data.Date;
        occorrenza.OraInizio = inizio;
        occorrenza.OraFine = fine;
        occorrenza.Note = model.Note?.Trim();

        await _db.SaveChangesAsync();

        TempData["Success"] = "Sessione aggiornata.";
        return RedirectToAction(nameof(Dettaglio), new { id = model.Id });
    }

    // ─────────────────────── ANNULLA / RIPRISTINA (soft-delete) ───────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Annulla(int id, string? motivo)
    {
        var occorrenza = await _db.OccorrenzeCorso.FindAsync(id);
        if (occorrenza is null) return NotFound();

        // Soft-delete: l'occorrenza non viene mai rimossa fisicamente, le presenze restano intatte.
        occorrenza.Stato = StatoOccorrenza.Annullata;
        occorrenza.DataAnnullamento = DateTime.Now;
        occorrenza.MotivoAnnullamento = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();

        await _db.SaveChangesAsync();

        TempData["Success"] = "Sessione annullata. I dati storici restano consultabili.";
        return RedirectToAction(nameof(Index), new { corsoId = occorrenza.TipologiaCorsoId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ripristina(int id)
    {
        var occorrenza = await _db.OccorrenzeCorso
            .Include(o => o.Presenze)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (occorrenza is null) return NotFound();

        // Se erano già state registrate presenze torna "Svolta", altrimenti "Programmata".
        occorrenza.Stato = occorrenza.Presenze.Any()
            ? StatoOccorrenza.Svolta
            : StatoOccorrenza.Programmata;
        occorrenza.DataAnnullamento = null;
        occorrenza.MotivoAnnullamento = null;

        await _db.SaveChangesAsync();

        TempData["Success"] = "Sessione ripristinata.";
        return RedirectToAction(nameof(Index), new { corsoId = occorrenza.TipologiaCorsoId });
    }

    // ─────────────────────── HELPERS ───────────────────────

    private async Task<OccorrenzaDettaglioViewModel> BuildDettaglioAsync(OccorrenzaCorso occorrenza)
    {
        // Iscritti correnti al corso, in ordine alfabetico
        var iscritti = await _db.IscrizioniCorso
            .Where(i => i.TipologiaCorsoId == occorrenza.TipologiaCorsoId)
            .Include(i => i.Cliente)
            .OrderBy(i => i.Cliente.Cognome).ThenBy(i => i.Cliente.Nome)
            .Select(i => i.Cliente)
            .ToListAsync();

        var presenze = occorrenza.Presenze.ToDictionary(p => p.ClienteId);

        var partecipanti = iscritti.Select(c => new RigaPresenzaViewModel
        {
            ClienteId = c.Id,
            NomeCompleto = c.NomeCompleto,
            Email = c.Email,
            PresenzaRegistrata = presenze.ContainsKey(c.Id),
            Presente = presenze.TryGetValue(c.Id, out var p) && p.Presente
        }).ToList();

        return new OccorrenzaDettaglioViewModel
        {
            Occorrenza = occorrenza,
            Corso = occorrenza.TipologiaCorso,
            Partecipanti = partecipanti
        };
    }
}
