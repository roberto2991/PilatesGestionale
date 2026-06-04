using System.ComponentModel.DataAnnotations;

namespace PilatesStudio.Models;

/// <summary>
/// Presenza di un Cliente a una specifica <see cref="OccorrenzaCorso"/>.
/// Costituisce un dato storico: non viene mai cancellata a cascata né per
/// eliminazione del corso né per annullamento dell'occorrenza.
/// </summary>
public class PresenzaCorso
{
    public int Id { get; set; }

    public int OccorrenzaCorsoId { get; set; }
    public OccorrenzaCorso OccorrenzaCorso { get; set; } = null!;

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    [Display(Name = "Presente")]
    public bool Presente { get; set; }

    [MaxLength(300)]
    [Display(Name = "Note")]
    public string? Note { get; set; }

    public DateTime DataRegistrazione { get; set; } = DateTime.Now;

    /// <summary>Username/email dell'operatore che ha registrato la presenza.</summary>
    [MaxLength(256)]
    public string? RegistrataDa { get; set; }
}
