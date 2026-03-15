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

        // Rename Identity tables (optional clean naming)
        builder.Entity<ApplicationUser>().ToTable("Utenti");
    }
}
