using PilatesStudio.Models;

namespace PilatesStudio.Services.Email;

public interface IEmailService
{
    Task<bool> InviaAsync(EmailMessage messaggio, CancellationToken ct = default);
}

public record EmailMessage(
    string Destinatario,
    string Oggetto,
    string CorpoHtml,
    TipoNotifica Tipo
);
