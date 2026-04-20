using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PilatesStudio.Data;
using PilatesStudio.Models;

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

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account");

        var insegnante = await _db.Insegnanti
            .FirstOrDefaultAsync(i => i.ApplicationUserId == user.Id);

        if (insegnante is null)
            return View("ErroreProfiloNonTrovato");

        return View(insegnante);
    }
}
