using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PilatesStudio.Data;
using PilatesStudio.Models;
using PilatesStudio.Models.ViewModels;
using PilatesStudio.Services;

namespace PilatesStudio.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class ClientiController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly DocumentoPdfService _pdfService;
    private const int PageSize = 10;
    private static readonly string[] TipiImmagineConsentiti = ["image/jpeg", "image/png", "image/gif", "image/webp"];
    private const long DimensioneMaxBytes = 5 * 1024 * 1024; // 5 MB

    public ClientiController(ApplicationDbContext context, IWebHostEnvironment env, DocumentoPdfService pdfService)
    {
        _context = context;
        _env = env;
        _pdfService = pdfService;
    }

    private async Task<string?> SalvaFotoAsync(IFormFile foto, string? vecchioPath = null)
    {
        if (!TipiImmagineConsentiti.Contains(foto.ContentType))
        {
            ModelState.AddModelError("Foto", "Formato non supportato. Usa JPG, PNG, GIF o WebP.");
            return null;
        }
        if (foto.Length > DimensioneMaxBytes)
        {
            ModelState.AddModelError("Foto", "L'immagine non può superare i 5 MB.");
            return null;
        }

        EliminaFileFoto(vecchioPath);

        var cartella = Path.Combine(_env.WebRootPath, "uploads", "clienti");
        Directory.CreateDirectory(cartella);

        var estensione = Path.GetExtension(foto.FileName).ToLowerInvariant();
        var nomeFile = $"{Guid.NewGuid()}{estensione}";
        var percorsoAssoluto = Path.Combine(cartella, nomeFile);

        await using var stream = new FileStream(percorsoAssoluto, FileMode.Create);
        await foto.CopyToAsync(stream);

        return $"/uploads/clienti/{nomeFile}";
    }

    private void EliminaFileFoto(string? relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return;
        var assoluto = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(assoluto))
            System.IO.File.Delete(assoluto);
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
    public async Task<IActionResult> Create(Cliente cliente, IFormFile? foto)
    {
        if (!ModelState.IsValid) return View(cliente);

        // Check email duplicata
        if (await _context.Clienti.AnyAsync(c => c.Email == cliente.Email))
        {
            ModelState.AddModelError("Email", "Esiste già un cliente con questa email.");
            return View(cliente);
        }

        if (foto is { Length: > 0 })
        {
            var path = await SalvaFotoAsync(foto);
            if (!ModelState.IsValid) return View(cliente);
            cliente.FotoProfiloPath = path;
        }

        cliente.DataIscrizione = DateTime.UtcNow;
        cliente.UltimoAggiornamento = DateTime.UtcNow;
        _context.Clienti.Add(cliente);
        await _context.SaveChangesAsync();

        // Genera il PDF di iscrizione precompilato
        try
        {
            cliente.DocumentoContrattoPath = _pdfService.GeneraContratto(cliente);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // La creazione del cliente va a buon fine anche senza PDF
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<ClientiController>>();
            logger.LogWarning(ex, "Impossibile generare il PDF di iscrizione per il cliente {Id}.", cliente.Id);
        }

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
    public async Task<IActionResult> Edit(int id, Cliente cliente, IFormFile? foto, bool rimuoviFoto = false)
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

        if (foto is { Length: > 0 })
        {
            var path = await SalvaFotoAsync(foto, existing.FotoProfiloPath);
            if (!ModelState.IsValid) return View(existing);
            existing.FotoProfiloPath = path;
        }
        else if (rimuoviFoto)
        {
            EliminaFileFoto(existing.FotoProfiloPath);
            existing.FotoProfiloPath = null;
        }

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

        EliminaFileFoto(cliente.FotoProfiloPath);
        _pdfService.EliminaContratto(cliente.DocumentoContrattoPath);
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

    // GET: Clienti/DownloadContratto/5
    public async Task<IActionResult> DownloadContratto(int id)
    {
        var cliente = await _context.Clienti.FindAsync(id);
        if (cliente == null) return NotFound();

        // Se il PDF non esiste ancora, lo generiamo al volo
        if (string.IsNullOrEmpty(cliente.DocumentoContrattoPath))
        {
            try
            {
                cliente.DocumentoContrattoPath = _pdfService.GeneraContratto(cliente);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<ClientiController>>();
                logger.LogError(ex, "Impossibile generare il PDF per il cliente {Id}.", id);
                TempData["Error"] = "Impossibile generare il documento PDF.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        var percorsoAssoluto = Path.Combine(
            _env.WebRootPath,
            cliente.DocumentoContrattoPath!.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        if (!System.IO.File.Exists(percorsoAssoluto))
        {
            // File mancante: rigenera
            try
            {
                cliente.DocumentoContrattoPath = _pdfService.GeneraContratto(cliente);
                await _context.SaveChangesAsync();
                percorsoAssoluto = Path.Combine(
                    _env.WebRootPath,
                    cliente.DocumentoContrattoPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            }
            catch
            {
                TempData["Error"] = "Impossibile generare il documento PDF.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        var nomeDownload = $"Iscrizione_{cliente.Cognome}_{cliente.Nome}_{cliente.DataIscrizione:yyyyMMdd}.pdf";
        var fileBytes = await System.IO.File.ReadAllBytesAsync(percorsoAssoluto);
        return File(fileBytes, "application/pdf", nomeDownload);
    }

    // POST: Clienti/RigeneraContratto/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RigeneraContratto(int id)
    {
        var cliente = await _context.Clienti.FindAsync(id);
        if (cliente == null) return NotFound();

        _pdfService.EliminaContratto(cliente.DocumentoContrattoPath);
        cliente.DocumentoContrattoPath = _pdfService.GeneraContratto(cliente, cliente.FirmaPath);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Documento PDF rigenerato con successo.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: Clienti/FirmaContratto/5
    public async Task<IActionResult> FirmaContratto(int id)
    {
        var cliente = await _context.Clienti.FindAsync(id);
        if (cliente == null) return NotFound();

        // Assicura che il PDF esista prima di mostrare la pagina di firma
        if (string.IsNullOrEmpty(cliente.DocumentoContrattoPath))
        {
            cliente.DocumentoContrattoPath = _pdfService.GeneraContratto(cliente);
            await _context.SaveChangesAsync();
        }

        return View(cliente);
    }

    // POST: Clienti/SalvaFirma/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvaFirma(int id, [FromForm] string signatureData)
    {
        if (string.IsNullOrWhiteSpace(signatureData))
            return BadRequest("Firma mancante.");

        var cliente = await _context.Clienti.FindAsync(id);
        if (cliente == null) return NotFound();

        // Salva PNG firma su disco
        var firmaCartella = Path.Combine(_env.WebRootPath, "uploads", "clienti", "firme");
        Directory.CreateDirectory(firmaCartella);

        var base64 = signatureData.Contains(',')
            ? signatureData[(signatureData.IndexOf(',') + 1)..]
            : signatureData;
        var bytes = Convert.FromBase64String(base64);

        var nomeFile = $"firma_{cliente.Id}_{Guid.NewGuid():N}.png";
        var percorsoAssoluto = Path.Combine(firmaCartella, nomeFile);
        await System.IO.File.WriteAllBytesAsync(percorsoAssoluto, bytes);

        // Elimina vecchia firma se presente
        _pdfService.EliminaFirma(cliente.FirmaPath);
        cliente.FirmaPath = $"/uploads/clienti/firme/{nomeFile}";

        // Rigenera PDF incorporando la firma
        _pdfService.EliminaContratto(cliente.DocumentoContrattoPath);
        cliente.DocumentoContrattoPath = _pdfService.GeneraContratto(cliente, cliente.FirmaPath);
        cliente.UltimoAggiornamento = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Json(new { success = true, redirectUrl = Url.Action(nameof(Details), new { id }) });
    }
}
