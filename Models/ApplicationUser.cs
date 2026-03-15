using Microsoft.AspNetCore.Identity;

namespace PilatesStudio.Models;

public class ApplicationUser : IdentityUser
{
    public string NomeCompleto { get; set; } = string.Empty;
    public DateTime DataCreazione { get; set; } = DateTime.UtcNow;
}
