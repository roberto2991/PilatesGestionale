using System.ComponentModel.DataAnnotations;

namespace PilatesStudio.Models;

public class Cliente
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Il nome è obbligatorio")]
    [StringLength(100)]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Il cognome è obbligatorio")]
    [StringLength(100)]
    [Display(Name = "Cognome")]
    public string Cognome { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'email è obbligatoria")]
    [EmailAddress(ErrorMessage = "Email non valida")]
    [StringLength(200)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Numero di telefono non valido")]
    [StringLength(20)]
    [Display(Name = "Telefono")]
    public string? Telefono { get; set; }

    [Display(Name = "Data di Nascita")]
    [DataType(DataType.Date)]
    public DateTime? DataNascita { get; set; }

    [StringLength(16)]
    [Display(Name = "Codice Fiscale")]
    public string? CodiceFiscale { get; set; }

    [StringLength(300)]
    [Display(Name = "Indirizzo")]
    public string? Indirizzo { get; set; }

    [StringLength(100)]
    [Display(Name = "Città")]
    public string? Citta { get; set; }

    [StringLength(5)]
    [Display(Name = "CAP")]
    public string? Cap { get; set; }

    [Display(Name = "Note")]
    [StringLength(1000)]
    public string? Note { get; set; }

    [Display(Name = "Attivo")]
    public bool Attivo { get; set; } = true;

    [Display(Name = "Data Iscrizione")]
    public DateTime DataIscrizione { get; set; } = DateTime.UtcNow;

    [Display(Name = "Ultimo Aggiornamento")]
    public DateTime UltimoAggiornamento { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    [Display(Name = "Foto Profilo")]
    public string? FotoProfiloPath { get; set; }

    [StringLength(500)]
    [Display(Name = "Documento Contratto")]
    public string? DocumentoContrattoPath { get; set; }

    [StringLength(500)]
    [Display(Name = "Firma Digitale")]
    public string? FirmaPath { get; set; }

    // Computed
    public string NomeCompleto => $"{Nome} {Cognome}";
    
    public ICollection<Abbonamento> Abbonamenti { get; set; } = new List<Abbonamento>();
}
