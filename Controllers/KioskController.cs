using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PilatesStudio.Data;
using PilatesStudio.Services;

namespace PilatesStudio.Controllers;

/// <summary>
/// Controller del kiosk iPad — tutte le route sono pubbliche (no autenticazione).
/// L'iPad punta sempre a /Kiosk e riceve i documenti da firmare tramite polling.
/// </summary>
[AllowAnonymous]
public class KioskController : Controller
{
    private readonly KioskStateService _kioskState;
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly DocumentoPdfService _pdfService;

    public KioskController(
        KioskStateService kioskState,
        ApplicationDbContext context,
        IWebHostEnvironment env,
        DocumentoPdfService pdfService)
    {
        _kioskState = kioskState;
        _context = context;
        _env = env;
        _pdfService = pdfService;
    }

    // GET /Kiosk  — pagina di benvenuto (indirizzo fisso sull'iPad)
    public IActionResult Index() => View();

    // GET /Kiosk/Stato  — API di polling, ritorna il clienteId in attesa (o null) e l'ultimo firmato
    public IActionResult Stato()
        => Json(new { clienteId = _kioskState.PendingClienteId, lastSignedClienteId = _kioskState.LastSignedClienteId });

    // GET /Kiosk/Firma/{id}  — pagina di firma del contratto per il cliente
    public async Task<IActionResult> Firma(int id)
    {
        var cliente = await _context.Clienti.FindAsync(id);
        if (cliente == null) return NotFound();

        // Assicura che il PDF esista
        if (string.IsNullOrEmpty(cliente.DocumentoContrattoPath))
        {
            cliente.DocumentoContrattoPath = _pdfService.GeneraContratto(cliente);
            await _context.SaveChangesAsync();
        }

        return View(cliente);
    }

    // POST /Kiosk/SalvaFirma/{id}  — salva la firma, rigenera PDF, libera il kiosk
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

        // Aggiorna il cliente
        _pdfService.EliminaFirma(cliente.FirmaPath);
        cliente.FirmaPath = $"/uploads/clienti/firme/{nomeFile}";

        _pdfService.EliminaContratto(cliente.DocumentoContrattoPath);
        cliente.DocumentoContrattoPath = _pdfService.GeneraContratto(cliente, cliente.FirmaPath);
        cliente.UltimoAggiornamento = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Registra la firma completata e libera il kiosk → tornerà alla welcome page
        _kioskState.SetFirmato(id);
        _kioskState.Clear();

        return Json(new { success = true });
    }

    // POST /Kiosk/Annulla  — annulla la sessione di firma (sia da iPad che da admin)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Annulla()
    {
        _kioskState.Clear();
        return Json(new { success = true });
    }
}
