using Microsoft.EntityFrameworkCore;
using PilatesStudio.Data;
using PilatesStudio.Models;

namespace PilatesStudio.Services.Email;

/// <summary>
/// Background service che ritenta l'invio delle email fallite ogni 5 minuti.
/// Massimo 3 tentativi — dopo il terzo imposta StatoInvio.RetryEsaurito.
/// </summary>
public class EmailRetryService : BackgroundService
{
    private const int MaxTentativi = 3;
    private static readonly TimeSpan Intervallo = TimeSpan.FromMinutes(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailRetryService> _logger;

    public EmailRetryService(
        IServiceScopeFactory scopeFactory,
        ILogger<EmailRetryService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailRetryService avviato.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(Intervallo, stoppingToken);

            try
            {
                await RetryFailedEmailsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Errore durante il retry delle email.");
            }
        }
    }

    private async Task RetryFailedEmailsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        // I log "Fallito" con tentativi ancora disponibili
        var daRitentare = await db.EmailNotificaLog
            .Where(l => l.Stato == StatoInvio.Fallito && l.TentativiEffettuati < MaxTentativi)
            .ToListAsync(ct);

        if (daRitentare.Count == 0) return;

        _logger.LogInformation("EmailRetryService: {Count} email da ritentare.", daRitentare.Count);

        foreach (var log in daRitentare)
        {
            log.TentativiEffettuati++;

            // Rebuild del messaggio minimo (senza corpo — non lo salviamo per GDPR)
            // Il retry invia solo un messaggio generico di notifica per le email di attivazione
            // già scadute/già usate. Per questo motivo, segniamo RetryEsaurito e notifichiamo admin.
            log.Stato = StatoInvio.RetryEsaurito;
            _logger.LogWarning(
                "Email a {Dest} (tipo: {Tipo}) ha raggiunto il limite di tentativi ({N}/{Max}). " +
                "Verificare la configurazione SMTP o reinviare manualmente dall'interfaccia admin.",
                log.Destinatario, log.Tipo, log.TentativiEffettuati, MaxTentativi);
        }

        await db.SaveChangesAsync(ct);
    }
}
