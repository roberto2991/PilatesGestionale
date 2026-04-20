using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PilatesStudio.Data;
using PilatesStudio.Models;
using PilatesStudio.Models.ViewModels;
using PilatesStudio.Services;
using PilatesStudio.Services.Email;

namespace PilatesStudio.Controllers;

[Authorize(Roles = "Admin")]
public class InsegnantiController : Controller
{
    private const int PageSize = 10;

    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly EmailTemplateService _emailTemplate;
    private readonly TokenAttivazioneService _tokenService;
    private readonly ILogger<InsegnantiController> _logger;

    public InsegnantiController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        EmailTemplateService emailTemplate,
        TokenAttivazioneService tokenService,
        ILogger<InsegnantiController> logger)
    {
        _db = db;
        _userManager = userManager;
        _emailService = emailService;
        _emailTemplate = emailTemplate;
        _tokenService = tokenService;
        _logger = logger;
    }

    // ─────────────────────── INDEX ───────────────────────

    [HttpGet]
    public async Task<IActionResult> Index(string? ricerca, int pagina = 1)
    {
        var query = _db.Insegnanti.AsQueryable();

        if (!string.IsNullOrWhiteSpace(ricerca))
        {
            var r = ricerca.Trim().ToLower();
            query = query.Where(i =>
                i.Nome.ToLower().Contains(r) ||
                i.Cognome.ToLower().Contains(r) ||
                i.Email.ToLower().Contains(r) ||
                i.CodiceFiscale.ToLower().Contains(r));
        }

        var totale = await query.CountAsync();
        var insegnanti = await query
            .OrderBy(i => i.Cognome).ThenBy(i => i.Nome)
            .Skip((pagina - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        return View(new InsegnanteListViewModel
        {
            Insegnanti = insegnanti,
            Ricerca = ricerca,
            PaginaCorrente = pagina,
            TotalePagine = (int)Math.Ceiling(totale / (double)PageSize),
            TotaleInsegnanti = totale
        });
    }

    // ─────────────────────── DETAILS ───────────────────────

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var insegnante = await _db.Insegnanti
            .Include(i => i.ApplicationUser)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (insegnante is null) return NotFound();

        // Recupera l'ultimo log email per questo insegnante
        var ultimaEmail = insegnante.Email.Length > 0
            ? await _db.EmailNotificaLog
                .Where(l => l.Destinatario == insegnante.Email
                         && l.Tipo == TipoNotifica.AttivazioneAccount)
                .OrderByDescending(l => l.CreatedAtUtc)
                .FirstOrDefaultAsync()
            : null;

        // Token attivo (non utilizzato, non scaduto)
        var tokenAttivo = insegnante.ApplicationUserId != null
            ? await _db.TokenAttivazioneAccount
                .Where(t => t.ApplicationUserId == insegnante.ApplicationUserId
                         && !t.Utilizzato
                         && t.ScadenzaUtc > DateTime.UtcNow)
                .AnyAsync()
            : false;

        ViewBag.UltimaEmail = ultimaEmail;
        ViewBag.TokenAttivo = tokenAttivo;
        return View(insegnante);
    }

    // ─────────────────────── CREATE ───────────────────────

    [HttpGet]
    public IActionResult Create() => View(new InsegnanteCreateViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InsegnanteCreateViewModel model)
    {
        // Verifica unicità email e CF
        if (await _db.Insegnanti.AnyAsync(i => i.Email == model.Email))
            ModelState.AddModelError(nameof(model.Email), "Email già presente nel sistema.");

        if (await _db.Insegnanti.AnyAsync(i => i.CodiceFiscale == model.CodiceFiscale.ToUpper()))
            ModelState.AddModelError(nameof(model.CodiceFiscale), "Codice fiscale già presente.");

        if (!ModelState.IsValid) return View(model);

        // 1. Crea ApplicationUser con password temporanea random (non sarà mai usata)
        var tempPassword = $"Temp_{Guid.NewGuid():N}!A1";
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            NomeCompleto = $"{model.Nome} {model.Cognome}",
            EmailConfirmed = false
        };

        var createResult = await _userManager.CreateAsync(user, tempPassword);
        if (!createResult.Succeeded)
        {
            foreach (var err in createResult.Errors)
                ModelState.AddModelError(string.Empty, err.Description);
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, "Insegnante");

        // 2. Crea entità Insegnante
        var insegnante = new Insegnante
        {
            Nome = model.Nome.Trim(),
            Cognome = model.Cognome.Trim(),
            CodiceFiscale = model.CodiceFiscale.ToUpper().Trim(),
            Email = model.Email.Trim(),
            Indirizzo = model.Indirizzo?.Trim(),
            TitoloDiStudio = model.TitoloDiStudio?.Trim(),
            StatoContratto = model.StatoContratto,
            ApplicationUserId = user.Id
        };
        _db.Insegnanti.Add(insegnante);
        await _db.SaveChangesAsync();

        // 3. Genera token attivazione
        var (tokenGrezzo, tokenEntity) = _tokenService.CreaToken(user.Id);
        _db.TokenAttivazioneAccount.Add(tokenEntity);
        await _db.SaveChangesAsync();

        // 4. Invia email (non blocca la creazione se fallisce)
        var corpo = _emailTemplate.GeneraEmailAttivazioneAccount(
            insegnante.NomeCompleto, tokenGrezzo);

        var inviato = await _emailService.InviaAsync(new EmailMessage(
            insegnante.Email,
            "Attiva il tuo account – Studio Pilates",
            corpo,
            TipoNotifica.AttivazioneAccount));

        TempData["Success"] = inviato
            ? $"Insegnante {insegnante.NomeCompleto} creata. Email di attivazione inviata a {insegnante.Email}."
            : $"Insegnante {insegnante.NomeCompleto} creata, ma l'invio dell'email è fallito. " +
              $"Usa il pulsante 'Reinvia invito' nella pagina dettaglio.";

        if (!inviato)
            TempData["Error"] = "Invio email fallito — controlla la configurazione SMTP o reinvia manualmente.";

        return RedirectToAction(nameof(Details), new { id = insegnante.Id });
    }

    // ─────────────────────── EDIT ───────────────────────

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var ins = await _db.Insegnanti.FindAsync(id);
        if (ins is null) return NotFound();

        return View(new InsegnanteEditViewModel
        {
            Id = ins.Id,
            Nome = ins.Nome,
            Cognome = ins.Cognome,
            CodiceFiscale = ins.CodiceFiscale,
            Email = ins.Email,
            Indirizzo = ins.Indirizzo,
            TitoloDiStudio = ins.TitoloDiStudio,
            StatoContratto = ins.StatoContratto
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(InsegnanteEditViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var ins = await _db.Insegnanti.FindAsync(model.Id);
        if (ins is null) return NotFound();

        // Verifica unicità su altri record
        if (await _db.Insegnanti.AnyAsync(i => i.Email == model.Email && i.Id != model.Id))
            ModelState.AddModelError(nameof(model.Email), "Email già in uso da un altro insegnante.");

        if (await _db.Insegnanti.AnyAsync(i =>
            i.CodiceFiscale == model.CodiceFiscale.ToUpper() && i.Id != model.Id))
            ModelState.AddModelError(nameof(model.CodiceFiscale), "Codice fiscale già in uso.");

        if (!ModelState.IsValid) return View(model);

        ins.Nome = model.Nome.Trim();
        ins.Cognome = model.Cognome.Trim();
        ins.CodiceFiscale = model.CodiceFiscale.ToUpper().Trim();
        ins.Indirizzo = model.Indirizzo?.Trim();
        ins.TitoloDiStudio = model.TitoloDiStudio?.Trim();
        ins.StatoContratto = model.StatoContratto;
        ins.UltimoAggiornamento = DateTime.UtcNow;

        // Se l'email cambia, aggiorna anche l'ApplicationUser
        if (ins.Email != model.Email.Trim() && ins.ApplicationUserId != null)
        {
            var user = await _userManager.FindByIdAsync(ins.ApplicationUserId);
            if (user != null)
            {
                user.Email = model.Email.Trim();
                user.UserName = model.Email.Trim();
                user.NomeCompleto = $"{ins.Nome} {ins.Cognome}";
                await _userManager.UpdateAsync(user);
            }
            ins.Email = model.Email.Trim();
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Dati insegnante aggiornati.";
        return RedirectToAction(nameof(Details), new { id = ins.Id });
    }

    // ─────────────────────── DELETE ───────────────────────

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var ins = await _db.Insegnanti.FindAsync(id);
        if (ins is null) return NotFound();
        return View(ins);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var ins = await _db.Insegnanti.FindAsync(id);
        if (ins is null) return NotFound();

        // Elimina utenza Identity collegata
        if (ins.ApplicationUserId != null)
        {
            var user = await _userManager.FindByIdAsync(ins.ApplicationUserId);
            if (user != null) await _userManager.DeleteAsync(user);
        }

        _db.Insegnanti.Remove(ins);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Insegnante {ins.NomeCompleto} eliminata.";
        return RedirectToAction(nameof(Index));
    }

    // ─────────────────────── REINVIA INVITO ───────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReinviaInvito(int id)
    {
        var ins = await _db.Insegnanti
            .Include(i => i.ApplicationUser)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (ins is null) return NotFound();

        if (ins.AccountAttivato)
        {
            TempData["Error"] = "L'account è già stato attivato — non è possibile reinviare l'invito.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (ins.ApplicationUserId is null)
        {
            TempData["Error"] = "Utenza non trovata. Eliminare e ricreare l'insegnante.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Invalida token precedenti
        await _tokenService.InvalidaTokenPrecedentiAsync(ins.ApplicationUserId, _db);

        // Genera nuovo token
        var (tokenGrezzo, tokenEntity) = _tokenService.CreaToken(ins.ApplicationUserId);
        _db.TokenAttivazioneAccount.Add(tokenEntity);
        await _db.SaveChangesAsync();

        // Re-invia email
        var corpo = _emailTemplate.GeneraEmailAttivazioneAccount(ins.NomeCompleto, tokenGrezzo);
        var inviato = await _emailService.InviaAsync(new EmailMessage(
            ins.Email,
            "Attiva il tuo account – Studio Pilates",
            corpo,
            TipoNotifica.AttivazioneAccount));

        TempData[inviato ? "Success" : "Error"] = inviato
            ? $"Nuovo invito inviato a {ins.Email}."
            : "Invio email fallito. Controlla la configurazione SMTP.";

        return RedirectToAction(nameof(Details), new { id });
    }
}
