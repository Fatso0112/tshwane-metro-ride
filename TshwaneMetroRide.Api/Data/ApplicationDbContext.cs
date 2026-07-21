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

        modelBuilder.Entity<BusCard>(entity =>
        {
            entity.HasKey(card => card.Id);

            entity.Property(card => card.CardNumber)
                .HasMaxLength(30)
                .IsRequired();

            entity.HasIndex(card => card.CardNumber)
                .IsUnique();

            entity.Property(card => card.Balance)
                .HasPrecision(12, 2);

            entity.Property(card => card.IsActive)
                .HasDefaultValue(true);

            entity.HasOne(card => card.Passenger)
                .WithMany(passenger => passenger.BusCards)
                .HasForeignKey(card => card.PassengerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}