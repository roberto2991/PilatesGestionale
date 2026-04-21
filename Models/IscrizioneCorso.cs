namespace PilatesStudio.Models;

/// <summary>Iscrizione di un Cliente attivo a una TipologiaCorso.</summary>
public class IscrizioneCorso
{
    public int Id { get; set; }

    public int TipologiaCorsoId { get; set; }
    public TipologiaCorso TipologiaCorso { get; set; } = null!;

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public DateTime DataIscrizione { get; set; } = DateTime.UtcNow;
}
