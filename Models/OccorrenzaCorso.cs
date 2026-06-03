using System.ComponentModel.DataAnnotations;

namespace PilatesStudio.Models;

/// <summary>
/// Singola sessione DATATA di un corso (es. "Lunedì 8 giugno 2026, 09:00–10:00").
/// Viene generata automaticamente espandendo le <see cref="SessioneCorso"/> (template
/// settimanali ricorrenti) sul periodo del corso. Ogni occorrenza è modificabile e
/// annullabile (soft-delete) singolarmente, senza intaccare le altre né il corso padre.
/// È l'unità su cui l'istruttrice registra le presenze.
/// </summary>
public class OccorrenzaCorso
{
    public int Id { get; set; }

    public int TipologiaCorsoId { get; set; }
    public TipologiaCorso TipologiaCorso { get; set; } = null!;

    /// <summary>Data del calendario in cui si svolge la sessione (componente data, senza orario).</summary>
    [DataType(DataType.Date)]
    [Display(Name = "Data")]
    public DateTime Data { get; set; }

    [Display(Name = "Ora Inizio")]
    public TimeSpan OraInizio { get; set; }

    [Display(Name = "Ora Fine")]
    public TimeSpan OraFine { get; set; }

    [Display(Name = "Stato")]
    public StatoOccorrenza Stato { get; set; } = StatoOccorrenza.Programmata;

    [MaxLength(500)]
    [Display(Name = "Note")]
    public string? Note { get; set; }

    // Tracciamento annullamento (soft-delete): l'occorrenza non viene mai rimossa fisicamente
    public DateTime? DataAnnullamento { get; set; }

    [MaxLength(300)]
    [Display(Name = "Motivo annullamento")]
    public string? MotivoAnnullamento { get; set; }

    public DateTime DataCreazione { get; set; } = DateTime.Now;

    // Navigation
    public ICollection<PresenzaCorso> Presenze { get; set; } = new List<PresenzaCorso>();

    // ─────────────────────── Computed ───────────────────────

    public DateTime InizioCompleto => Data.Date + OraInizio;

    public bool Annullata => Stato == StatoOccorrenza.Annullata;

    public string GiornoNome => Data.DayOfWeek switch
    {
        DayOfWeek.Monday    => "Lunedì",
        DayOfWeek.Tuesday   => "Martedì",
        DayOfWeek.Wednesday => "Mercoledì",
        DayOfWeek.Thursday  => "Giovedì",
        DayOfWeek.Friday    => "Venerdì",
        DayOfWeek.Saturday  => "Sabato",
        DayOfWeek.Sunday    => "Domenica",
        _                   => ""
    };

    public string OrarioFormatted => $"{OraInizio:hh\\:mm} – {OraFine:hh\\:mm}";
}

public enum StatoOccorrenza
{
    [Display(Name = "Programmata")]
    Programmata = 0,

    [Display(Name = "Svolta")]
    Svolta = 1,

    [Display(Name = "Annullata")]
    Annullata = 2
}
