using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PilatesStudio.Data;
using PilatesStudio.Models;

namespace PilatesStudio.Services;

public class TokenAttivazioneService
{
    private static readonly TimeSpan TokenDurata = TimeSpan.FromHours(72);

    /// <summary>
    /// Genera un nuovo token grezzo (da inviare via email) e la relativa entità da persistere.
    /// Il token grezzo NON deve mai essere salvato in DB — solo il suo hash SHA-256.
    /// </summary>
    public (string tokenGrezzo, TokenAttivazioneAccount entity) CreaToken(string applicationUserId)
    {
        var tokenGrezzo = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var hash = ComputaHash(tokenGrezzo);

        var entity = new TokenAttivazioneAccount
        {
            ApplicationUserId = applicationUserId,
            TokenHash = hash,
            ScadenzaUtc = DateTime.UtcNow.Add(TokenDurata)
        };

        return (tokenGrezzo, entity);
    }

    /// <summary>
    /// Verifica il token grezzo ricevuto dall'URL. Restituisce null se non valido o scaduto.
    /// </summary>
    public async Task<TokenAttivazioneAccount?> ValidaTokenAsync(
        string tokenGrezzo, ApplicationDbContext db, CancellationToken ct = default)
    {
        var hash = ComputaHash(tokenGrezzo);

        var token = await db.TokenAttivazioneAccount
            .Include(t => t.Utente)
            .FirstOrDefaultAsync(t => t.TokenHash == hash && !t.Utilizzato, ct);

        if (token is null) return null;
        if (token.ScadenzaUtc < DateTime.UtcNow) return null;

        return token;
    }

    /// <summary>
    /// Verifica solo se il token è scaduto (ma non ancora utilizzato), per mostrare il messaggio corretto.
    /// </summary>
    public async Task<bool> TokenScadutoAsync(
        string tokenGrezzo, ApplicationDbContext db, CancellationToken ct = default)
    {
        var hash = ComputaHash(tokenGrezzo);
        var token = await db.TokenAttivazioneAccount
            .FirstOrDefaultAsync(t => t.TokenHash == hash && !t.Utilizzato, ct);

        return token is not null && token.ScadenzaUtc < DateTime.UtcNow;
    }

    /// <summary>Segna il token come utilizzato (one-time use).</summary>
    public async Task ConsumaTokenAsync(TokenAttivazioneAccount token, ApplicationDbContext db)
    {
        token.Utilizzato = true;
        token.DataUtilizzo = DateTime.UtcNow;
        db.TokenAttivazioneAccount.Update(token);
        await db.SaveChangesAsync();
    }

    /// <summary>Invalida tutti i token attivi precedenti di un utente (usato al re-invio invito).</summary>
    public async Task InvalidaTokenPrecedentiAsync(
        string applicationUserId, ApplicationDbContext db, CancellationToken ct = default)
    {
        var tokenAttivi = await db.TokenAttivazioneAccount
            .Where(t => t.ApplicationUserId == applicationUserId && !t.Utilizzato)
            .ToListAsync(ct);

        foreach (var t in tokenAttivi)
        {
            t.Utilizzato = true;
            t.DataUtilizzo = DateTime.UtcNow;
        }

        if (tokenAttivi.Count > 0)
            await db.SaveChangesAsync(ct);
    }

    private static string ComputaHash(string input)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
}
