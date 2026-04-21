using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PilatesStudio.Models;

namespace PilatesStudio.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Cliente> Clienti { get; set; }
    public DbSet<Abbonamento> Abbonamenti { get; set; }
    public DbSet<Insegnante> Insegnanti { get; set; }
    public DbSet<TokenAttivazioneAccount> TokenAttivazioneAccount { get; set; }
    public DbSet<EmailNotificaLog> EmailNotificaLog { get; set; }

    // Gestione corsi
    public DbSet<TipologiaCorso> TipologieCorsi { get; set; }
    public DbSet<SessioneCorso> SessioniCorso { get; set; }
    public DbSet<TipologiaCorsoInsegnante> TipologieCorsoInsegnanti { get; set; }
    public DbSet<IscrizioneCorso> IscrizioniCorso { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Cliente
        builder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Cognome).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
        });

        // Abbonamento
        builder.Entity<Abbonamento>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Prezzo).HasColumnType("decimal(10,2)");
            entity.HasOne(e => e.Cliente)
                  .WithMany(c => c.Abbonamenti)
                  .HasForeignKey(e => e.ClienteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Insegnante
        builder.Entity<Insegnante>(e =>
        {
            e.HasIndex(x => x.CodiceFiscale).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Nome).IsRequired().HasMaxLength(100);
            e.Property(x => x.Cognome).IsRequired().HasMaxLength(100);
            e.Property(x => x.Email).IsRequired().HasMaxLength(200);
            e.Property(x => x.CodiceFiscale).IsRequired().HasMaxLength(16);
            e.Property(x => x.StatoContratto).HasConversion<string>();
            e.HasOne(x => x.ApplicationUser)
             .WithOne()
             .HasForeignKey<Insegnante>(x => x.ApplicationUserId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // Token attivazione
        builder.Entity<TokenAttivazioneAccount>(e =>
        {
            e.HasIndex(x => x.TokenHash).IsUnique();
            e.HasIndex(x => new { x.ApplicationUserId, x.Utilizzato });
            e.HasOne(x => x.Utente)
             .WithMany()
             .HasForeignKey(x => x.ApplicationUserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // TipologiaCorso
        builder.Entity<TipologiaCorso>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).IsRequired().HasMaxLength(150);
            e.Property(x => x.Descrizione).HasMaxLength(500);
        });

        // SessioneCorso
        builder.Entity<SessioneCorso>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.GiornoSettimana).HasConversion<int>();
            e.HasOne(x => x.TipologiaCorso)
             .WithMany(c => c.Sessioni)
             .HasForeignKey(x => x.TipologiaCorsoId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // TipologiaCorsoInsegnante (M:N junction)
        builder.Entity<TipologiaCorsoInsegnante>(e =>
        {
            e.HasKey(x => new { x.TipologiaCorsoId, x.InsegnanteId });
            e.HasOne(x => x.TipologiaCorso)
             .WithMany(c => c.TipologieCorsoInsegnanti)
             .HasForeignKey(x => x.TipologiaCorsoId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Insegnante)
             .WithMany()
             .HasForeignKey(x => x.InsegnanteId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // IscrizioneCorso
        builder.Entity<IscrizioneCorso>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TipologiaCorsoId, x.ClienteId }).IsUnique();
            e.HasOne(x => x.TipologiaCorso)
             .WithMany(c => c.Iscrizioni)
             .HasForeignKey(x => x.TipologiaCorsoId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Cliente)
             .WithMany()
             .HasForeignKey(x => x.ClienteId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Rename Identity tables (optional clean naming)
        builder.Entity<ApplicationUser>().ToTable("Utenti");
    }
}
