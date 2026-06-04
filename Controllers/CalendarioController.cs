using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PilatesStudio.Data;
using PilatesStudio.Models;
using PilatesStudio.Models.ViewModels;

namespace PilatesStudio.Controllers;

/// <summary>
/// Calendario grafico (Toast UI Calendar) delle occorrenze dei corsi.
/// Admin/Staff vedono tutti i corsi; le insegnanti solo quelli a loro assegnati.
/// Il click su un evento riporta al dettaglio dell'occorrenza, dove i permessi
/// vengono comunque rivalidati lato server.
/// </summary>
[Authorize(Roles = "Admin,Staff,Insegnante")]
public class CalendarioController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public CalendarioController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    private bool IsInsegnanteSemplice() =>
        User.IsInRole("Insegnante") && !User.IsInRole("Admin") && !User.IsInRole("Staff");

    /// <summary>
    /// Id dei corsi visibili all'utente corrente. Restituisce null per Admin/Staff
    /// (nessun filtro = tutti i corsi).
    /// </summary>
    private async Task<List<int>?> CorsiVisibiliAsync()
    {
        if (!IsInsegnanteSemplice()) return null;

        var userId = _userManager.GetUserId(User);
        if (userId is null) return new List<int>();

        return await _db.TipologieCorsoInsegnanti
            .Where(t => t.Insegnante.ApplicationUserId == userId)
            .Select(t => t.TipologiaCorsoId)
            .ToListAsync();
    }

    // ─────────────────────── INDEX (vista calendario) ───────────────────────

    [HttpGet]
    public async Task<IActionResult> Index(int? corsoId)
    {
        var visibili = await CorsiVisibiliAsync();

        // L'insegnante non può filtrare su un corso non assegnato.
        if (corsoId.HasValue && visibili is not null && !visibili.Contains(corsoId.Value))
            return Forbid();

        var query = _db.TipologieCorsi.AsQueryable();
        if (visibili is not null)
            query = query.Where(c => visibili.Contains(c.Id));
        if (corsoId.HasValue)
            query = query.Where(c => c.Id == corsoId.Value);

        var corsi = await query
            .OrderBy(c => c.Nome)
            .Select(c => new CalendarioCorsoDto
            {
                Id = c.Id,
                Nome = c.Nome,
                Colore = string.IsNullOrEmpty(c.Colore) ? "#3b82f6" : c.Colore
            })
            .ToListAsync();

        return View(new CalendarioViewModel
        {
            Corsi = corsi,
            CorsoIdFiltro = corsoId,
            NomeCorsoFiltro = corsoId.HasValue ? corsi.FirstOrDefault()?.Nome : null
        });
    }

    // ─────────────────────── EVENTI (JSON per Toast UI) ───────────────────────

    [HttpGet]
    public async Task<IActionResult> Eventi(DateTime start, DateTime end, int? corsoId)
    {
        var visibili = await CorsiVisibiliAsync();

        var query = _db.OccorrenzeCorso
            .Include(o => o.TipologiaCorso)
            .Where(o => o.Data >= start.Date && o.Data < end.Date.AddDays(1));

        if (visibili is not null)
            query = query.Where(o => visibili.Contains(o.TipologiaCorsoId));
        if (corsoId.HasValue)
            query = query.Where(o => o.TipologiaCorsoId == corsoId.Value);

        var occorrenze = await query.ToListAsync();

        var eventi = occorrenze.Select(o =>
        {
            var annullata = o.Annullata;
            var colore = annullata
                ? "#9ca3af"
                : (string.IsNullOrEmpty(o.TipologiaCorso.Colore) ? "#3b82f6" : o.TipologiaCorso.Colore);

            return new
            {
                id = o.Id.ToString(),
                calendarId = o.TipologiaCorsoId.ToString(),
                title = (annullata ? "✕ " : "") + o.TipologiaCorso.Nome,
                category = "time",
                start = (o.Data.Date + o.OraInizio).ToString("yyyy-MM-ddTHH:mm:ss"),
                end = (o.Data.Date + o.OraFine).ToString("yyyy-MM-ddTHH:mm:ss"),
                backgroundColor = colore,
                borderColor = colore,
                dragBackgroundColor = colore,
                isReadOnly = true
            };
        });

        return Json(eventi);
    }
}
