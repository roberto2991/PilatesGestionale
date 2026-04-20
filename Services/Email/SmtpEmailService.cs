using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using PilatesStudio.Data;
using PilatesStudio.Models;

namespace PilatesStudio.Services.Email;

public class SmtpEmailService : IEmailService
{
    private readonly EmailOptions _opts;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(
        IOptions<EmailOptions> opts,
        IServiceScopeFactory scopeFactory,
        ILogger<SmtpEmailService> logger)
    {
        _opts = opts.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<bool> InviaAsync(EmailMessage msg, CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var log = new EmailNotificaLog
        {
            Destinatario = msg.Destinatario,
            Oggetto = msg.Oggetto,
            Tipo = msg.Tipo,
            Stato = StatoInvio.InCoda
        };
        db.EmailNotificaLog.Add(log);
        await db.SaveChangesAsync(ct);

        try
        {
            using var client = new SmtpClient();

            var secureSocketOptions = _opts.UseSsl
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTlsWhenAvailable;

            await client.ConnectAsync(_opts.Host, _opts.Port, secureSocketOptions, ct);
            await client.AuthenticateAsync(_opts.Username, _opts.Password, ct);

            var mimeMsg = new MimeMessage();
            mimeMsg.From.Add(new MailboxAddress(_opts.FromName, _opts.FromAddress));
            mimeMsg.To.Add(MailboxAddress.Parse(msg.Destinatario));
            mimeMsg.Subject = msg.Oggetto;
            mimeMsg.Body = new TextPart(TextFormat.Html) { Text = msg.CorpoHtml };

            await client.SendAsync(mimeMsg, ct);
            await client.DisconnectAsync(true, ct);

            log.Stato = StatoInvio.Inviato;
            log.InviatoAtUtc = DateTime.UtcNow;
            _logger.LogInformation("Email inviata a {Dest} (tipo: {Tipo})", msg.Destinatario, msg.Tipo);
        }
        catch (Exception ex)
        {
            log.Stato = StatoInvio.Fallito;
            log.ErroreDettaglio = ex.Message;
            _logger.LogError(ex, "Invio email fallito a {Dest}", msg.Destinatario);
        }

        db.EmailNotificaLog.Update(log);
        await db.SaveChangesAsync(ct);
        return log.Stato == StatoInvio.Inviato;
    }
}
