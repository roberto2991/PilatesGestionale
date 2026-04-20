namespace PilatesStudio.Models;

public class EmailNotificaLog
{
    public int Id { get; set; }
    public string Destinatario { get; set; } = "";
    public string Oggetto { get; set; } = "";
    public TipoNotifica Tipo { get; set; }
    public StatoInvio Stato { get; set; }
    public string? ErroreDettaglio { get; set; }
    public int TentativiEffettuati { get; set; } = 1;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? InviatoAtUtc { get; set; }
}

public enum TipoNotifica
{
    AttivazioneAccount = 1,
    ResetPassword = 2,
    ReminderScadenzaContratto = 3
}

public enum StatoInvio
{
    InCoda = 0,
    Inviato = 1,
    Fallito = 2,
    RetryEsaurito = 3
}
