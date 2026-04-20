using System.ComponentModel.DataAnnotations;

namespace PilatesStudio.Models;

public class Insegnante
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Il nome è obbligatorio")]
    [MaxLength(100)]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = "";

    [Required(ErrorMessage = "Il cognome è obbligatorio")]
    [MaxLength(100)]
    [Display(Name = "Cognome")]
    public string Cognome { get; set; } = "";

    [Required(ErrorMessage = "Il codice fiscale è obbligatorio")]
    [MaxLength(16)]
    [RegularExpression(@"^[A-Z]{6}[0-9]{2}[A-Z][0-9]{2}[A-Z][0-9]{3}[A-Z]$",
        ErrorMessage = "Codice fiscale non valido")]
    [Display(Name = "Codice Fiscale")]
    public string CodiceFiscale { get; set; } = "";

    [Required(ErrorMessage = "L'email è obbligatoria")]
    [EmailAddress(ErrorMessage = "Email non valida")]
    [MaxLength(200)]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [MaxLength(300)]
    [Display(Name = "Indirizzo")]
    public string? Indirizzo { get; set; }

    [MaxLength(200)]
    [Display(Name = "Titolo di Studio")]
    public string? TitoloDiStudio { get; set; }

    [Display(Name = "Stato Contrattuale")]
    public StatoContrattuale StatoContratto { get; set; } = StatoContrattuale.Bozza;

    // Collegamento all'utenza Identity
    public string? ApplicationUserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }

    public bool AccountAttivato { get; set; } = false;
    public DateTime? DataAttivazione { get; set; }
    public DateTime DataCreazione { get; set; } = DateTime.UtcNow;
    public DateTime UltimoAggiornamento { get; set; } = DateTime.UtcNow;

    // Computed
    public string NomeCompleto => $"{Nome} {Cognome}";
}

public enum StatoContrattuale
{
    [Display(Name = "Bozza")]
    Bozza = 0,

    [Display(Name = "Attivo")]
    Attivo = 1,

    [Display(Name = "Sospeso")]
    Sospeso = 2,

    [Display(Name = "Cessato")]
    Cessato = 3
}
