using System.ComponentModel.DataAnnotations;

namespace PilatesStudio.Models;

public class TipologiaCorso
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Il nome è obbligatorio")]
    [MaxLength(150)]
    [Display(Name = "Nome Corso")]
    public string Nome { get; set; } = "";

    [MaxLength(500)]
    [Display(Name = "Descrizione")]
    public string? Descrizione { get; set; }

    [Required(ErrorMessage = "La capacità massima è obbligatoria")]
    [Range(1, 200, ErrorMessage = "La capacità deve essere compresa tra 1 e 200")]
    [Display(Name = "Capacità Massima")]
    public int CapacitaMax { get; set; } = 10;

    [Required(ErrorMessage = "La data di inizio è obbligatoria")]
    [DataType(DataType.Date)]
    [Display(Name = "Data Inizio")]
    public DateTime DataInizio { get; set; }

    [Required(ErrorMessage = "La data di fine è obbligatoria")]
    [DataType(DataType.Date)]
    [Display(Name = "Data Fine")]
    public DateTime DataFine { get; set; }

    [Display(Name = "Attivo")]
    public bool Attivo { get; set; } = true;

    /// <summary>Colore identificativo del corso (formato HEX, es. "#3b82f6"), usato nel calendario.</summary>
    [MaxLength(7)]
    [RegularExpression(@"^#(?:[0-9a-fA-F]{6})$", ErrorMessage = "Colore non valido (formato #RRGGBB)")]
    [Display(Name = "Colore")]
    public string Colore { get; set; } = "#3b82f6";

    /// <summary>
    /// Corso archiviato: usato quando esiste storico di presenze che ne impedisce
    /// l'eliminazione fisica. I dati restano intatti ma il corso è "congelato".
    /// </summary>
    [Display(Name = "Archiviato")]
    public bool Archiviato { get; set; } = false;

    public DateTime? DataArchiviazione { get; set; }

    public DateTime DataCreazione { get; set; } = DateTime.UtcNow;
    public DateTime UltimoAggiornamento { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<SessioneCorso> Sessioni { get; set; } = new List<SessioneCorso>();
    public ICollection<OccorrenzaCorso> Occorrenze { get; set; } = new List<OccorrenzaCorso>();
    public ICollection<TipologiaCorsoInsegnante> TipologieCorsoInsegnanti { get; set; } = new List<TipologiaCorsoInsegnante>();
    public ICollection<IscrizioneCorso> Iscrizioni { get; set; } = new List<IscrizioneCorso>();
}
