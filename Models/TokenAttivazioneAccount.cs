using System.ComponentModel.DataAnnotations;

namespace PilatesStudio.Models;

public class TokenAttivazioneAccount
{
    public int Id { get; set; }

    [Required]
    public string ApplicationUserId { get; set; } = "";
    public ApplicationUser Utente { get; set; } = null!;

    /// <summary>SHA-256 del token grezzo — il token grezzo non viene mai persistito.</summary>
    [Required]
    public string TokenHash { get; set; } = "";

    public DateTime ScadenzaUtc { get; set; }
    public bool Utilizzato { get; set; } = false;
    public DateTime? DataUtilizzo { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
