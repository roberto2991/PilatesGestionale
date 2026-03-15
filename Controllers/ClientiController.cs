using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PilatesStudio.Data;
using PilatesStudio.Models;
using PilatesStudio.Models.ViewModels;

namespace PilatesStudio.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class ClientiController : Controller
{
    private readonly ApplicationDbContext _context;
    private const int PageSize = 10;

    public ClientiController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Clienti
    public async Task<IActionResult> Index(string? ricerca, bool? soloAttivi, int pagina = 1)
    {
        var query = _context.Clienti.AsQueryable();

        if (!string.IsNullOrWhiteSpace(ricerca))
            query = query.Where(c => c.Nome.Contains(ricerca)
                                  || c.Cognome.Contains(ricerca)
                                  || c.Email.Contains(ricerca)
                                  || (c.Telefono != null && c.Telefono.Contains(ricerca)));

        if (soloAttivi == true)
            query = query.Where(c => c.Attivo);

        var totale = await query.CountAsync();
        var clienti = await query
            .OrderBy(c => c.Cognome).ThenBy(c => c.Nome)
            .Skip((pagina - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        var vm = new ClienteListViewModel
        {
            Clienti = clienti,
            Ricerca = ricerca,
            SoloAttivi = soloAttivi,
            PaginaCorrente = pagina,
            TotalePagine = (int)Math.Ceiling(totale / (double)PageSize),
            TotaleClienti = totale
        };

        return View(vm);
    }

    // GET: Clienti/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var cliente = await _context.Clienti
            .Include(c => c.Abbonamenti)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cliente == null) return NotFound();
        return View(cliente);
    }

    // GET: Clienti/Create
    public IActionResult Create() => View(new Cliente());

    // POST: Clienti/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Cliente cliente)
    {
        if (!ModelState.IsValid) return View(cliente);

        // Check email duplicata
        if (await _context.Clienti.AnyAsync(c => c.Email == cliente.Email))
        {
            ModelState.AddModelError("Email", "Esiste già un cliente con questa email.");
            return View(cliente);
        }

        cliente.DataIscrizione = DateTime.UtcNow;
        cliente.UltimoAggiornamento = DateTime.UtcNow;
        _context.Clienti.Add(cliente);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Cliente {cliente.NomeCompleto} creato con successo!";
        return RedirectToAction(nameof(Index));
    }

    // GET: Clienti/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var cliente = await _context.Clienti.FindAsync(id);
        if (cliente == null) return NotFound();
        return View(cliente);
    }

    // POST: Clienti/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Cliente cliente)
    {
        if (id != cliente.Id) return BadRequest();
        if (!ModelState.IsValid) return View(cliente);

        // Check email duplicata (escludi se stessa)
        if (await _context.Clienti.AnyAsync(c => c.Email == cliente.Email && c.Id != id))
        {
            ModelState.AddModelError("Email", "Esiste già un altro cliente con questa email.");
            return View(cliente);
        }

        var existing = await _context.Clienti.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Nome = cliente.Nome;
        existing.Cognome = cliente.Cognome;
        existing.Email = cliente.Email;
        existing.Telefono = cliente.Telefono;
        existing.DataNascita = cliente.DataNascita;
        existing.CodiceFiscale = cliente.CodiceFiscale;
        existing.Indirizzo = cliente.Indirizzo;
        existing.Citta = cliente.Citta;
        existing.Cap = cliente.Cap;
        existing.Note = cliente.Note;
        existing.Attivo = cliente.Attivo;
        existing.UltimoAggiornamento = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        TempData["Success"] = $"Cliente {existing.NomeCompleto} aggiornato con successo!";
        return RedirectToAction(nameof(Index));
    }

    // GET: Clienti/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _context.Clienti.FindAsync(id);
        if (cliente == null) return NotFound();
        return View(cliente);
    }

    // POST: Clienti/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var cliente = await _context.Clienti.FindAsync(id);
        if (cliente == null) return NotFound();

        _context.Clienti.Remove(cliente);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Cliente eliminato con successo.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Clienti/ToggleAttivo/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAttivo(int id)
    {
        var cliente = await _context.Clienti.FindAsync(id);
        if (cliente == null) return NotFound();

        cliente.Attivo = !cliente.Attivo;
        cliente.UltimoAggiornamento = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Stato cliente aggiornato.";
        return RedirectToAction(nameof(Index));
    }
}
