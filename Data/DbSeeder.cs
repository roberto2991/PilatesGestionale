using Microsoft.AspNetCore.Identity;
using PilatesStudio.Models;

namespace PilatesStudio.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(UserManager<ApplicationUser> userManager,
                                       RoleManager<IdentityRole> roleManager)
    {
        // Crea ruolo Admin se non esiste
        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        if (!await roleManager.RoleExistsAsync("Staff"))
            await roleManager.CreateAsync(new IdentityRole("Staff"));

        // Crea utente Admin di default
        const string adminEmail = "admin@pilatesstudio.it";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                NomeCompleto = "Amministratore",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
