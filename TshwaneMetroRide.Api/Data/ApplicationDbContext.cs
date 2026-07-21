using Microsoft.EntityFrameworkCore;
using TshwaneMetroRide.Api.Models;

namespace TshwaneMetroRide.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Passenger> Passengers => Set<Passenger>();

    public DbSet<BusCard> BusCards => Set<BusCard>();

    public DbSet<WalletTransaction> WalletTransactions => 
        Set<WalletTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Passenger>(entity =>
        {
            entity.HasKey(passenger => passenger.Id);

            entity.Property(passenger => passenger.FullName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(passenger => passenger.Email)
                .HasMaxLength(200)
                .IsRequired();

            entity.HasIndex(passenger => passenger.Email)
                .IsUnique();

            entity.Property(passenger => passenger.PhoneNumber)
                .HasMaxLength(20);

            entity.Property(passenger => passenger.PasswordHash)
                .HasMaxLength(500)
                .IsRequired();
        });
    }
}