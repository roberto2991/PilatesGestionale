using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PilatesStudio.Models;

public enum TipoAbbonamento
{
    [Display(Name = "Lezione Singola")]
    Singola = 1,
    [Display(Name = "5 Lezioni")]
    Cinque = 2,
    [Display(Name = "10 Lezioni")]
    Dieci = 3,
    [Display(Name = "Mensile")]
    Mensile = 4,
    [Display(Name = "Trimestrale")]
    Trimestrale = 5,
    [Display(Name = "Annuale")]
    Annuale = 6
}

public enum StatoAbbonamento
{
    [Display(Name = "Attivo")]
    Attivo = 1,
    [Display(Name = "Scaduto")]
    Scaduto = 2,
    [Display(Name = "Sospeso")]
    Sospeso = 3,
    [Display(Name = "Annullato")]
    Annullato = 4
}

public class Abbonamento
{
    public int Id { get; set; }

    [Required]
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    [Required(ErrorMessage = "Il tipo abbonamento è obbligatorio")]
    [Display(Name = "Tipo Abbonamento")]
    public TipoAbbonamento Tipo { get; set; }

    [Required(ErrorMessage = "La data inizio è obbligatoria")]
    [Display(Name = "Data Inizio")]
    [DataType(DataType.Date)]
    public DateTime DataInizio { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "La data scadenza è obbligatoria")]
    [Display(Name = "Data Scadenza")]
    [DataType(DataType.Date)]
    public DateTime DataScadenza { get; set; }

    [Required(ErrorMessage = "Il prezzo è obbligatorio")]
    [Column(TypeName = "decimal(10,2)")]
    [Display(Name = "Prezzo (€)")]
    [Range(0, 9999.99)]
    public decimal Prezzo { get; set; }

    [Display(Name = "Lezioni Incluse")]
    public int? LezioniIncluse { get; set; }

    [Display(Name = "Lezioni Utilizzate")]
    public int LezioniUtilizzate { get; set; } = 0;

    [Display(Name = "Stato")]
    public StatoAbbonamento Stato { get; set; } = StatoAbbonamento.Attivo;

    [Display(Name = "Note")]
    [StringLength(500)]
    public string? Note { get; set; }

    public DateTime DataCreazione { get; set; } = DateTime.UtcNow;
}
