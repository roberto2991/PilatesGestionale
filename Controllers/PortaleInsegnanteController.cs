using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PilatesStudio.Data;
using PilatesStudio.Models;
using PilatesStudio.Models.ViewModels;

namespace PilatesStudio.Controllers;

[Authorize(Roles = "Insegnante")]
public class PortaleInsegnanteController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public PortaleInsegnanteController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
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

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account");

        var insegnante = await _db.Insegnanti
            .FirstOrDefaultAsync(i => i.ApplicationUserId == user.Id);

        if (insegnante is null)
            return View("ErroreProfiloNonTrovato");

        //inizio query lista corsi insegnante

        var query = _db.TipologieCorsi
            .Include(c => c.Sessioni)
            .AsQueryable();

        // L'insegnante vede solo i corsi a lei assegnati.
        if (IsInsegnanteSemplice())
        {
            var assegnati = await CorsiAssegnatiAsync();
            query = query.Where(c => assegnati.Contains(c.Id));
        }

        var corsi = await query
            .OrderBy(c => c.Nome)
            .ToListAsync();

        // Conta iscritti per ogni corso in una singola query
        var corsoIds = corsi.Select(c => c.Id).ToList();
        var conteggioIscritti = await _db.IscrizioniCorso
            .Where(i => corsoIds.Contains(i.TipologiaCorsoId))
            .GroupBy(i => i.TipologiaCorsoId)
            .Select(g => new { CorsoId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CorsoId, x => x.Count);
        // fine query

        return View(new CorsiAssegnatiInsegnante
        {
            Corsi = corsi,
            Insegnante = insegnante,
            NumeroIscrittiPerCorso = conteggioIscritti
        });
    }
}
