using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PilatesStudio.Data;
using PilatesStudio.Models;
using PilatesStudio.Models.ViewModels;
using PilatesStudio.Services;

namespace PilatesStudio.Controllers;

// Lettura (Index/Details) consentita anche alle insegnanti, limitatamente ai corsi assegnati.
// Le operazioni di scrittura (Create/Edit/Delete/iscrizioni) restano riservate agli Admin.
[Authorize(Roles = "Admin,Staff,Insegnante")]
public class CorsiController : Controller
{
    private const int PageSize = 10;

    private readonly ApplicationDbContext _db;
    private readonly ILogger<CorsiController> _logger;
    private readonly OccorrenzeCorsoService _occorrenze;
    private readonly UserManager<ApplicationUser> _userManager;

    public CorsiController(
        ApplicationDbContext db,
        ILogger<CorsiController> logger,
        OccorrenzeCorsoService occorrenze,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _logger = logger;
        _occorrenze = occorrenze;
        _userManager = userManager;
    }

    // ─────────────────────── HELPER PERMESSI INSEGNANTE ───────────────────────

    /// <summary>True se l'utente corrente è un'insegnante senza privilegi Admin/Staff.</summary>
    private bool IsInsegnanteSemplice() =>
        User.IsInRole("Insegnante") && !User.IsInRole("Admin") && !User.IsInRole("Staff");

    /// <summary>Id dei corsi assegnati all'insegnante collegata all'utente corrente.</summary>
    private async Task<List<int>> CorsiAssegnatiAsync()
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null) return new List<int>();

        return await _db.TipologieCorsoInsegnanti
            .Where(t => t.Insegnante.ApplicationUserId == userId)
            .Select(t => t.TipologiaCorsoId)
            .ToListAsync();
    }

    // ─────────────────────── INDEX ───────────────────────

    [HttpGet]
    public async Task<IActionResult> Index(string? ricerca, bool? soloAttivi, int pagina = 1)
    {
        var query = _db.TipologieCorsi
            .Include(c => c.Sessioni)
            .AsQueryable();

        // L'insegnante vede solo i corsi a lei assegnati.
        if (IsInsegnanteSemplice())
        {
            var assegnati = await CorsiAssegnatiAsync();
            query = query.Where(c => assegnati.Contains(c.Id));
        }

        if (!string.IsNullOrWhiteSpace(ricerca))
        {
            var r = ricerca.Trim().ToLower();
            query = query.Where(c => c.Nome.ToLower().Contains(r) ||
                                     (c.Descrizione != null && c.Descrizione.ToLower().Contains(r)));
        }

        if (soloAttivi == true)
            query = query.Where(c => c.Attivo);

        var totale = await query.CountAsync();
        var corsi = await query
            .OrderBy(c => c.Nome)
            .Skip((pagina - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        // Conta iscritti per ogni corso in una singola query
        var corsoIds = corsi.Select(c => c.Id).ToList();
        var conteggioIscritti = await _db.IscrizioniCorso
            .Where(i => corsoIds.Contains(i.TipologiaCorsoId))
            .GroupBy(i => i.TipologiaCorsoId)
            .Select(g => new { CorsoId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CorsoId, x => x.Count);

        return View(new CorsoListViewModel
        {
            Corsi = corsi,
            Ricerca = ricerca,
            SoloAttivi = soloAttivi,
            PaginaCorrente = pagina,
            TotalePagine = (int)Math.Ceiling(totale / (double)PageSize),
            TotaleCorsi = totale,
            NumeroIscrittiPerCorso = conteggioIscritti
        });
    }

    // ─────────────────────── DETAILS ───────────────────────

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        // L'insegnante può vedere solo i dettagli dei corsi a lei assegnati.
        if (IsInsegnanteSemplice() && !(await CorsiAssegnatiAsync()).Contains(id))
            return Forbid();

        var corso = await _db.TipologieCorsi
            .Include(c => c.Sessioni)
            .Include(c => c.TipologieCorsoInsegnanti)
                .ThenInclude(tci => tci.Insegnante)
            .Include(c => c.Iscrizioni)
                .ThenInclude(i => i.Cliente)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (corso is null) return NotFound();

        // Clienti attivi non ancora iscritti a questo corso
        var idIscritti = corso.Iscrizioni.Select(i => i.ClienteId).ToHashSet();
        var clientiIscrivibili = await _db.Clienti
            .Where(c => c.Attivo && !idIscritti.Contains(c.Id))
            .OrderBy(c => c.Cognome).ThenBy(c => c.Nome)
            .ToListAsync();

        return View(new CorsoDetailsViewModel
        {
            Corso = corso,
            Iscrizioni = corso.Iscrizioni.OrderBy(i => i.Cliente.Cognome).ThenBy(i => i.Cliente.Nome).ToList(),
            ClientiIscrivibili = clientiIscrivibili
        });
    }

    // ─────────────────────── CREATE ───────────────────────

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        var vm = new CorsoCreateViewModel
        {
            InsegnantiDisponibili = await InsegnantiAttiviAsync()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CorsoCreateViewModel model)
    {
        await ValidateDate(model.DataInizio, model.DataFine, nameof(model.DataFine));

        if (model.Sessioni.Count == 0)
            ModelState.AddModelError(string.Empty, "Aggiungere almeno una sessione settimanale.");

        ValidateSessions(model.Sessioni);

        if (!ModelState.IsValid)
        {
            model.InsegnantiDisponibili = await InsegnantiAttiviAsync();
            return View(model);
        }

        var corso = new TipologiaCorso
        {
            Nome = model.Nome.Trim(),
            Descrizione = model.Descrizione?.Trim(),
            CapacitaMax = model.CapacitaMax,
            DataInizio = model.DataInizio.ToUniversalTime(),
            DataFine = model.DataFine.ToUniversalTime(),
            Attivo = model.Attivo,
            Colore = model.Colore
        };

        _db.TipologieCorsi.Add(corso);
        await _db.SaveChangesAsync();

        // Sessioni
        foreach (var s in model.Sessioni)
        {
            _db.SessioniCorso.Add(new SessioneCorso
            {
                TipologiaCorsoId = corso.Id,
                GiornoSettimana = (DayOfWeek)s.GiornoSettimana,
                OraInizio = TimeSpan.Parse(s.OraInizio),
                OraFine = TimeSpan.Parse(s.OraFine)
            });
        }

        // Insegnanti
        foreach (var insId in model.InsegnantiSelezionati.Distinct())
        {
            _db.TipologieCorsoInsegnanti.Add(new TipologiaCorsoInsegnante
            {
                TipologiaCorsoId = corso.Id,
                InsegnanteId = insId
            });
        }

        await _db.SaveChangesAsync();

        // Genera automaticamente le occorrenze (sessioni datate) per l'intero periodo.
        // Usa le date locali del form per evitare drift di fuso nel calcolo dei giorni.
        var generate = await _occorrenze.SincronizzaAsync(
            corso.Id, model.DataInizio, model.DataFine, DateTime.Now);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Corso '{corso.Nome}' creato con successo. Generate {generate} sessioni in calendario.";
        return RedirectToAction(nameof(Details), new { id = corso.Id });
    }

    // ─────────────────────── EDIT ───────────────────────

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var corso = await _db.TipologieCorsi
            .Include(c => c.Sessioni)
            .Include(c => c.TipologieCorsoInsegnanti)
            .Include(c => c.Iscrizioni)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (corso is null) return NotFound();

        var vm = new CorsoEditViewModel
        {
            Id = corso.Id,
            Nome = corso.Nome,
            Descrizione = corso.Descrizione,
            CapacitaMax = corso.CapacitaMax,
            DataInizio = corso.DataInizio.ToLocalTime(),
            DataFine = corso.DataFine.ToLocalTime(),
            Attivo = corso.Attivo,
            Colore = string.IsNullOrWhiteSpace(corso.Colore) ? "#3b82f6" : corso.Colore,
            InsegnantiSelezionati = corso.TipologieCorsoInsegnanti.Select(t => t.InsegnanteId).ToList(),
            Sessioni = corso.Sessioni.Select(s => new SessioneCorsoInputModel
            {
                GiornoSettimana = (int)s.GiornoSettimana,
                OraInizio = s.OraInizio.ToString(@"hh\:mm"),
                OraFine = s.OraFine.ToString(@"hh\:mm")
            }).ToList(),
            InsegnantiDisponibili = await InsegnantiAttiviAsync(),
            NumeroIscrittiAttuali = corso.Iscrizioni.Count
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(CorsoEditViewModel model)
    {
        await ValidateDate(model.DataInizio, model.DataFine, nameof(model.DataFine));

        if (model.Sessioni.Count == 0)
            ModelState.AddModelError(string.Empty, "Aggiungere almeno una sessione settimanale.");

        ValidateSessions(model.Sessioni);

        // Capacità non può scendere sotto il numero di iscritti attuali
        var numIscritti = await _db.IscrizioniCorso.CountAsync(i => i.TipologiaCorsoId == model.Id);
        if (model.CapacitaMax < numIscritti)
            ModelState.AddModelError(nameof(model.CapacitaMax),
                $"La capacità non può essere inferiore al numero di iscritti attuali ({numIscritti}).");

        if (!ModelState.IsValid)
        {
            model.InsegnantiDisponibili = await InsegnantiAttiviAsync();
            model.NumeroIscrittiAttuali = numIscritti;
            return View(model);
        }

        var corso = await _db.TipologieCorsi.FindAsync(model.Id);
        if (corso is null) return NotFound();

        corso.Nome = model.Nome.Trim();
        corso.Descrizione = model.Descrizione?.Trim();
        corso.CapacitaMax = model.CapacitaMax;
        corso.DataInizio = model.DataInizio.ToUniversalTime();
        corso.DataFine = model.DataFine.ToUniversalTime();
        corso.Attivo = model.Attivo;
        corso.Colore = model.Colore;
        corso.UltimoAggiornamento = DateTime.UtcNow;

        // Sostituisci sessioni
        var sessioniVecchie = await _db.SessioniCorso
            .Where(s => s.TipologiaCorsoId == model.Id)
            .ToListAsync();
        _db.SessioniCorso.RemoveRange(sessioniVecchie);

        foreach (var s in model.Sessioni)
        {
            _db.SessioniCorso.Add(new SessioneCorso
            {
                TipologiaCorsoId = corso.Id,
                GiornoSettimana = (DayOfWeek)s.GiornoSettimana,
                OraInizio = TimeSpan.Parse(s.OraInizio),
                OraFine = TimeSpan.Parse(s.OraFine)
            });
        }

        // Sostituisci insegnanti
        var insegnantiVecchi = await _db.TipologieCorsoInsegnanti
            .Where(t => t.TipologiaCorsoId == model.Id)
            .ToListAsync();
        _db.TipologieCorsoInsegnanti.RemoveRange(insegnantiVecchi);

        foreach (var insId in model.InsegnantiSelezionati.Distinct())
        {
            _db.TipologieCorsoInsegnanti.Add(new TipologiaCorsoInsegnante
            {
                TipologiaCorsoId = corso.Id,
                InsegnanteId = insId
            });
        }

        await _db.SaveChangesAsync();

        // Riallinea il calendario delle occorrenze al nuovo periodo/orari, preservando lo storico:
        // aggiunge le sessioni mancanti e rimuove solo quelle future, senza presenze e non annullate.
        await _occorrenze.SincronizzaAsync(
            corso.Id, model.DataInizio, model.DataFine, DateTime.Now);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Corso '{corso.Nome}' aggiornato.";
        return RedirectToAction(nameof(Details), new { id = corso.Id });
    }

    // ─────────────────────── DELETE ───────────────────────

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var corso = await _db.TipologieCorsi
            .Include(c => c.Iscrizioni)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (corso is null) return NotFound();

        // Se esistono presenze registrate il corso non è eliminabile fisicamente: verrà archiviato.
        ViewBag.HaPresenze = await _db.PresenzeCorso
            .AnyAsync(p => p.OccorrenzaCorso.TipologiaCorsoId == id);

        return View(corso);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var corso = await _db.TipologieCorsi.FindAsync(id);
        if (corso is null) return NotFound();

        // Regola di business: un corso con presenze registrate non può essere eliminato
        // fisicamente, ma viene archiviato mantenendo intatto tutto lo storico.
        var haPresenze = await _db.PresenzeCorso
            .AnyAsync(p => p.OccorrenzaCorso.TipologiaCorsoId == id);

        if (haPresenze)
        {
            corso.Archiviato = true;
            corso.Attivo = false;
            corso.DataArchiviazione = DateTime.UtcNow;
            corso.UltimoAggiornamento = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Corso '{corso.Nome}' archiviato: sono presenti dati storici di " +
                                  "presenze, quindi non è stato eliminato. Tutti i dati restano consultabili.";
            return RedirectToAction(nameof(Index));
        }

        // Nessuna presenza: eliminazione fisica (la cascata rimuove occorrenze, sessioni, iscrizioni).
        _db.TipologieCorsi.Remove(corso);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Corso '{corso.Nome}' eliminato.";
        return RedirectToAction(nameof(Index));
    }

    // ─────────────────────── ISCRIZIONI ───────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> IscriviCliente(int corsoId, int clienteId)
    {
        var corso = await _db.TipologieCorsi
            .Include(c => c.Iscrizioni)
            .FirstOrDefaultAsync(c => c.Id == corsoId);

        if (corso is null) return NotFound();

        var cliente = await _db.Clienti.FindAsync(clienteId);
        if (cliente is null || !cliente.Attivo)
        {
            TempData["Error"] = "Il cliente selezionato non è attivo o non esiste.";
            return RedirectToAction(nameof(Details), new { id = corsoId });
        }

        if (corso.Iscrizioni.Count >= corso.CapacitaMax)
        {
            TempData["Error"] = "Capacità massima raggiunta. Impossibile iscrivere altri clienti.";
            return RedirectToAction(nameof(Details), new { id = corsoId });
        }

        if (corso.Iscrizioni.Any(i => i.ClienteId == clienteId))
        {
            TempData["Error"] = "Il cliente è già iscritto a questo corso.";
            return RedirectToAction(nameof(Details), new { id = corsoId });
        }

        _db.IscrizioniCorso.Add(new IscrizioneCorso
        {
            TipologiaCorsoId = corsoId,
            ClienteId = clienteId
        });
        await _db.SaveChangesAsync();

        TempData["Success"] = $"{cliente.NomeCompleto} iscritto al corso con successo.";
        return RedirectToAction(nameof(Details), new { id = corsoId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RimuoviIscrizione(int iscrizioneId, int corsoId)
    {
        var iscrizione = await _db.IscrizioniCorso
            .Include(i => i.Cliente)
            .FirstOrDefaultAsync(i => i.Id == iscrizioneId);

        if (iscrizione is null) return NotFound();

        _db.IscrizioniCorso.Remove(iscrizione);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"{iscrizione.Cliente.NomeCompleto} rimosso dal corso.";
        return RedirectToAction(nameof(Details), new { id = corsoId });
    }

    // ─────────────────────── HELPERS ───────────────────────

    private async Task<List<Insegnante>> InsegnantiAttiviAsync() =>
        await _db.Insegnanti
            .Where(i => i.StatoContratto == StatoContrattuale.Attivo)
            .OrderBy(i => i.Cognome).ThenBy(i => i.Nome)
            .ToListAsync();

    private Task ValidateDate(DateTime dataInizio, DateTime dataFine, string fieldName)
    {
        if (dataFine <= dataInizio)
            ModelState.AddModelError(fieldName, "La data di fine deve essere successiva alla data di inizio.");
        return Task.CompletedTask;
    }

    private void ValidateSessions(List<SessioneCorsoInputModel> sessioni)
    {
        for (int i = 0; i < sessioni.Count; i++)
        {
            var s = sessioni[i];
            if (TimeSpan.TryParse(s.OraInizio, out var start) &&
                TimeSpan.TryParse(s.OraFine, out var end) &&
                end <= start)
            {
                ModelState.AddModelError(string.Empty,
                    $"Sessione {i + 1}: l'ora di fine deve essere successiva all'ora di inizio.");
            }
        }
    }
}
