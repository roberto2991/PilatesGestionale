using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PilatesStudio.Data;
using PilatesStudio.Models;
using PilatesStudio.Models.ViewModels;

namespace PilatesStudio.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Dashboard()
    {
        var ora = DateTime.UtcNow;
        var inizioMese = new DateTime(ora.Year, ora.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var inizioAnno = new DateTime(ora.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var vm = new DashboardViewModel
        {
            TotaleClienti = await _context.Clienti.CountAsync(),
            ClientiAttivi = await _context.Clienti.CountAsync(c => c.Attivo),
            NuoviClientiMese = await _context.Clienti
                .CountAsync(c => c.DataIscrizione >= inizioMese),
            AbbonatiAttivi = await _context.Abbonamenti
                .CountAsync(a => a.Stato == StatoAbbonamento.Attivo && a.DataScadenza >= ora),
            IncassoMese = await _context.Abbonamenti
                .Where(a => a.DataCreazione >= inizioMese)
                .SumAsync(a => (decimal?)a.Prezzo) ?? 0,
            IncassoAnno = await _context.Abbonamenti
                .Where(a => a.DataCreazione >= inizioAnno)
                .SumAsync(a => (decimal?)a.Prezzo) ?? 0,
            UltimiClienti = await _context.Clienti
                .OrderByDescending(c => c.DataIscrizione)
                .Take(5)
                .Select(c => new ClienteRecenteDto
                {
                    Id = c.Id,
                    NomeCompleto = c.Nome + " " + c.Cognome,
                    Email = c.Email,
                    DataIscrizione = c.DataIscrizione,
                    Attivo = c.Attivo
                }).ToListAsync(),
            AbbonatiInScadenza = await _context.Abbonamenti
                .Include(a => a.Cliente)
                .Where(a => a.Stato == StatoAbbonamento.Attivo
                         && a.DataScadenza >= ora
                         && a.DataScadenza <= ora.AddDays(30))
                .OrderBy(a => a.DataScadenza)
                .Take(5)
                .Select(a => new AbbonamentoScadenzaDto
                {
                    ClienteId = a.ClienteId,
                    NomeCliente = a.Cliente.Nome + " " + a.Cliente.Cognome,
                    TipoAbbonamento = a.Tipo.ToString(),
                    DataScadenza = a.DataScadenza,
                    GiorniRimanenti = (int)(a.DataScadenza - ora).TotalDays
                }).ToListAsync()
        };

        return View(vm);
    }
}
