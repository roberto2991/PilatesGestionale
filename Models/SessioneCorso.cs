namespace PilatesStudio.Models;

public class SessioneCorso
{
    public int Id { get; set; }

    public int TipologiaCorsoId { get; set; }
    public TipologiaCorso TipologiaCorso { get; set; } = null!;

    public DayOfWeek GiornoSettimana { get; set; }

    public TimeSpan OraInizio { get; set; }

    public TimeSpan OraFine { get; set; }

    // Computed
    public string GiornoNome => GiornoSettimana switch
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

    public string OrarioFormatted =>
        $"{OraInizio:hh\\:mm} – {OraFine:hh\\:mm}";
}
