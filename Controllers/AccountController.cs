using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PilatesStudio.Data;
using PilatesStudio.Models;
using PilatesStudio.Models.ViewModels;
using PilatesStudio.Services;

namespace PilatesStudio.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly TokenAttivazioneService _tokenService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        TokenAttivazioneService tokenService,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _db = db;
        _tokenService = tokenService;
        _logger = logger;
    }

    // ─────────────────────── LOGIN ───────────────────────

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Dashboard", "Admin");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);

        var result = await _signInManager.PasswordSignInAsync(
            model.Email, model.Password, model.RicordaMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("Utente {Email} ha effettuato il login.", model.Email);

            // Controlla se l'utente deve ancora cambiare la password
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var hasClaim = await _userManager.GetClaimsAsync(user);
                if (hasClaim.Any(c => c.Type == "MustChangePassword" && c.Value == "true"))
                    return RedirectToAction(nameof(ImpostaPassword));
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectByRole();
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account bloccato. Riprova tra qualche minuto.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Email o password non validi.");
        return View(model);
    }

    // ─────────────────────── LOGOUT ───────────────────────

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("Utente ha effettuato il logout.");
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    // ─────────────────────── PRIMO ACCESSO VIA LINK ───────────────────────

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> PrimoAccesso(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction(nameof(LinkNonValido));

        // Controlla se scaduto (per mostrare messaggio più preciso)
        if (await _tokenService.TokenScadutoAsync(token, _db))
            return RedirectToAction(nameof(LinkScaduto));

        var tokenEntity = await _tokenService.ValidaTokenAsync(token, _db);
        if (tokenEntity is null)
            return RedirectToAction(nameof(LinkNonValido));

        // Consuma il token (one-time use)
        await _tokenService.ConsumaTokenAsync(tokenEntity, _db);

        // Effettua il sign-in dell'utente (il link è il fattore di autenticazione)
        var user = tokenEntity.Utente;
        await _signInManager.SignInAsync(user, isPersistent: false);

        // Aggiunge claim temporaneo in-memory che impone il cambio password
        // (persistito su DB come claim utente, rimosso dopo ImpostaPassword)
        var claimResult = await _userManager.AddClaimAsync(
            user, new Claim("MustChangePassword", "true"));

        if (!claimResult.Succeeded)
            _logger.LogWarning("Impossibile aggiungere claim MustChangePassword a {UserId}", user.Id);

        // Rigenera il cookie di autenticazione per includere il nuovo claim
        await _signInManager.RefreshSignInAsync(user);

        _logger.LogInformation(
            "Primo accesso tramite link per utente {Email}. Reindirizzamento a ImpostaPassword.",
            user.Email);

        return RedirectToAction(nameof(ImpostaPassword));
    }

    // ─────────────────────── IMPOSTA PASSWORD ───────────────────────

    [HttpGet]
    [Authorize]
    public IActionResult ImpostaPassword() => View(new ImpostaPasswordViewModel());

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImpostaPassword(ImpostaPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction(nameof(Login));

        // Rimuove la password temporanea e imposta quella nuova
        var removeResult = await _userManager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded)
        {
            foreach (var err in removeResult.Errors)
                ModelState.AddModelError(string.Empty, err.Description);
            return View(model);
        }

        var addResult = await _userManager.AddPasswordAsync(user, model.NuovaPassword);
        if (!addResult.Succeeded)
        {
            foreach (var err in addResult.Errors)
                ModelState.AddModelError(string.Empty, err.Description);
            return View(model);
        }

        // Rimuove il claim MustChangePassword
        var claims = await _userManager.GetClaimsAsync(user);
        var mustChangeClaim = claims.FirstOrDefault(c => c.Type == "MustChangePassword");
        if (mustChangeClaim != null)
            await _userManager.RemoveClaimAsync(user, mustChangeClaim);

        // Marca l'account come attivato nell'entità Insegnante
        var insegnante = _db.Insegnanti
            .FirstOrDefault(i => i.ApplicationUserId == user.Id);
        if (insegnante != null)
        {
            insegnante.AccountAttivato = true;
            insegnante.DataAttivazione = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        // Conferma email
        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);

        // Rigenera il cookie senza il claim MustChangePassword
        await _signInManager.RefreshSignInAsync(user);

        _logger.LogInformation("Utente {Email} ha impostato la password e attivato l'account.", user.Email);

        TempData["Success"] = "Password impostata con successo! Benvenuta nel portale.";
        return RedirectByRole();
    }

    // ─────────────────────── LINK NON VALIDO / SCADUTO ───────────────────────

    [HttpGet]
    [AllowAnonymous]
    public IActionResult LinkNonValido() => View();

    [HttpGet]
    [AllowAnonymous]
    public IActionResult LinkScaduto() => View();

    // ─────────────────────── HELPERS ───────────────────────

    private IActionResult RedirectByRole()
    {
        if (User.IsInRole("Admin") || User.IsInRole("Staff"))
            return RedirectToAction("Dashboard", "Admin");

        if (User.IsInRole("Insegnante"))
            return RedirectToAction("Index", "PortaleInsegnante");

        return RedirectToAction("Index", "Home");
    }
}
