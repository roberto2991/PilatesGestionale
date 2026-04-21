namespace PilatesStudio.Models;

/// <summary>Tabella di giunzione M:N tra TipologiaCorso e Insegnante.</summary>
public class TipologiaCorsoInsegnante
{
    public int TipologiaCorsoId { get; set; }
    public TipologiaCorso TipologiaCorso { get; set; } = null!;

    public int InsegnanteId { get; set; }
    public Insegnante Insegnante { get; set; } = null!;
}
