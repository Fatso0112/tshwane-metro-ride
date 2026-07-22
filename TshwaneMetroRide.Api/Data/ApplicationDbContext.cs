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

    public DbSet<BusRoute> BusRoutes => Set<BusRoute>();

    public DbSet<WalletTransaction> WalletTransactions =>
        Set<WalletTransaction>();

    public DbSet<Ticket> Tickets => Set<Ticket>();

    public DbSet<Bus> Buses => Set<Bus>();

    public DbSet<SupportRequest> SupportRequests => Set<SupportRequest>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigurePassenger(modelBuilder);
        ConfigureBusCard(modelBuilder);
        ConfigureWalletTransaction(modelBuilder);
        ConfigureBusRoute(modelBuilder);
        ConfigureTicket(modelBuilder);
        ConfigureBus(modelBuilder);
        ConfigureSupportRequest(modelBuilder);
    }

    private static void ConfigurePassenger(
        ModelBuilder modelBuilder)
    {
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

    private static void ConfigureBusCard(
        ModelBuilder modelBuilder)
    {
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

            entity.Property(card => card.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active")
                .IsRequired();

            entity.HasOne(card => card.Passenger)
                .WithMany(passenger => passenger.BusCards)
                .HasForeignKey(card => card.PassengerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureWalletTransaction(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WalletTransaction>(entity =>
        {
            entity.HasKey(transaction => transaction.Id);

            entity.Property(transaction => transaction.Amount)
                .HasPrecision(12, 2);

            entity.Property(transaction => transaction.BalanceAfter)
                .HasPrecision(12, 2);

            entity.Property(transaction => transaction.TransactionType)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(transaction => transaction.Reference)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(transaction => transaction.Reference)
                .IsUnique();

            entity.HasIndex(transaction => new
            {
                transaction.BusCardId,
                transaction.CreatedAtUtc
            });

            entity.HasOne(transaction => transaction.BusCard)
                .WithMany(card => card.Transactions)
                .HasForeignKey(transaction => transaction.BusCardId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureBusRoute(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BusRoute>(entity =>
        {
            entity.HasKey(route => route.Id);

            entity.Property(route => route.RouteCode)
                .HasMaxLength(20)
                .IsRequired();

            entity.HasIndex(route => route.RouteCode)
                .IsUnique();

            entity.Property(route => route.RouteName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(route => route.Origin)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(route => route.Destination)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(route => route.Stops)
                .HasMaxLength(1000);

            entity.Property(route => route.FareAmount)
                .HasPrecision(12, 2);

            entity.Property(route => route.IsActive)
                .HasDefaultValue(true);

            entity.HasData(
                new BusRoute
                {
                    Id = 1,
                    RouteCode = "TMR-R001",
                    RouteName = "Soshanguve to Pretoria CBD",
                    Origin = "Soshanguve",
                    Destination = "Pretoria CBD",
                    Stops =
                        "Soshanguve, Mabopane, Akasia, Pretoria CBD",
                    FareAmount = 25.00m,
                    IsActive = true
                },
                new BusRoute
                {
                    Id = 2,
                    RouteCode = "TMR-R002",
                    RouteName = "Centurion to Pretoria CBD",
                    Origin = "Centurion",
                    Destination = "Pretoria CBD",
                    Stops =
                        "Centurion, Lyttelton, Groenkloof, Pretoria CBD",
                    FareAmount = 20.00m,
                    IsActive = true
                });
        });
    }

    private static void ConfigureTicket(
    ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(ticket => ticket.Id);

            entity.Property(ticket => ticket.TicketNumber)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(ticket => ticket.TicketNumber)
                .IsUnique();

            entity.Property(ticket => ticket.QrCodeValue)
                .HasMaxLength(200)
                .IsRequired();

            entity.HasIndex(ticket => ticket.QrCodeValue)
                .IsUnique();

            entity.Property(ticket => ticket.FareAmount)
                .HasPrecision(12, 2);

            entity.Property(ticket => ticket.Status)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(ticket => ticket.PassengerPhone)
                .HasMaxLength(20);

            entity.HasIndex(ticket => new
            {
                ticket.BusCardId,
                ticket.PurchasedAtUtc
            });

            entity.HasOne(ticket => ticket.BusCard)
                .WithMany(card => card.Tickets)
                .HasForeignKey(ticket => ticket.BusCardId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ticket => ticket.BusRoute)
                .WithMany(route => route.Tickets)
                .HasForeignKey(ticket => ticket.BusRouteId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureBus(
    ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bus>(entity =>
        {
            entity.HasKey(bus => bus.Id);

            entity.Property(bus => bus.FleetNumber)
                .HasMaxLength(30)
                .IsRequired();

            entity.HasIndex(bus => bus.FleetNumber)
                .IsUnique();

            entity.Property(bus => bus.RegistrationNumber)
                .HasMaxLength(20)
                .IsRequired();

            entity.HasIndex(bus => bus.RegistrationNumber)
                .IsUnique();

            entity.Property(bus => bus.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Active")
                .IsRequired();

            entity.HasIndex(bus => bus.Status);

            entity.HasIndex(bus => bus.BusRouteId);

            entity.HasOne(bus => bus.BusRoute)
                .WithMany(route => route.Buses)
                .HasForeignKey(bus => bus.BusRouteId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureSupportRequest(
    ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SupportRequest>(entity =>
        {
            entity.HasKey(request => request.Id);

            entity.Property(request => request.Subject)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(request => request.Message)
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(request => request.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Open")
                .IsRequired();

            entity.HasIndex(request => request.Status);

            entity.HasIndex(request => new
            {
                request.PassengerId,
                request.CreatedAtUtc
            });

            entity.HasOne(request => request.Passenger)
                .WithMany(passenger =>
                    passenger.SupportRequests)
                .HasForeignKey(request =>
                    request.PassengerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}