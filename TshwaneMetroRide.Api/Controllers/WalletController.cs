using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TshwaneMetroRide.Api.Data;
using TshwaneMetroRide.Api.DTOs.Wallet;
using TshwaneMetroRide.Api.Models;

namespace TshwaneMetroRide.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/wallet")]
public class WalletController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public WalletController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("cards/{cardId:int}/top-up")]
    public async Task<IActionResult> TopUpCard(
        int cardId,
        TopUpRequest request)
    {
        var passengerId = GetPassengerId();

        if (passengerId is null)
        {
            return Unauthorized(new
            {
                message = "The access token is invalid."
            });
        }

        var amount = decimal.Round(
            request.Amount,
            2,
            MidpointRounding.AwayFromZero);

        if (amount <= 0)
        {
            return BadRequest(new
            {
                message =
                    "The top-up amount must be greater than zero."
            });
        }

        if (amount > 10000)
        {
            return BadRequest(new
            {
                message =
                    "A single top-up cannot exceed R10,000."
            });
        }

        await using var databaseTransaction =
            await _context.Database.BeginTransactionAsync();

        var busCard = await _context.BusCards
            .Include(card => card.TravelWallet)
            .SingleOrDefaultAsync(card =>
                card.Id == cardId &&
                card.PassengerId == passengerId.Value);

        if (busCard is null)
        {
            return NotFound(new
            {
                message =
                    "The bus card was not found for this passenger."
            });
        }

        if (!busCard.IsActive || busCard.Status != "Active")
        {
            return BadRequest(new
            {
                message =
                    "This bus card is inactive and cannot be topped up."
            });
        }

        if (busCard.TravelWallet is null)
        {
            return BadRequest(new
            {
                message =
                    "This bus card does not have an associated travel wallet."
            });
        }

        if (busCard.TravelWallet.Status != "Active")
        {
            return BadRequest(new
            {
                message =
                    "The associated travel wallet is inactive and cannot be topped up."
            });
        }

        busCard.TravelWallet.Balance += request.Amount;
        busCard.TravelWallet.UpdatedAtUtc = DateTime.UtcNow;

        var reference =
            $"TMR-{DateTime.UtcNow:yyyyMMddHHmmss}-" +
            $"{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

        var walletTransaction = new WalletTransaction
        {
            Amount = request.Amount,
            TransactionType = "TopUp",
            Reference = reference,
            BalanceAfter = busCard.TravelWallet.Balance,
            CreatedAtUtc = DateTime.UtcNow,
            BusCardId = busCard.Id,
            TravelWalletId = busCard.TravelWallet.Id
        };

        _context.WalletTransactions.Add(walletTransaction);

        await _context.SaveChangesAsync();
        await databaseTransaction.CommitAsync();

        return Ok(new
        {
            message = "Bus card topped up successfully.",

            transaction = new
            {
                walletTransaction.Id,
                walletTransaction.Reference,
                walletTransaction.TransactionType,
                walletTransaction.Amount,
                walletTransaction.BalanceAfter,
                walletTransaction.CreatedAtUtc
            },

            busCard = new
            {
                busCard.Id,
                busCard.CardNumber,
                busCard.TravelWallet!.Balance,
                travelWalletId = busCard.TravelWallet.Id,
                walletStatus = busCard.TravelWallet.Status
            }
        });
    }

    [HttpGet("cards/{cardId:int}/transactions")]
    public async Task<IActionResult> GetTransactions(
        int cardId)
    {
        var passengerId = GetPassengerId();

        if (passengerId is null)
        {
            return Unauthorized(new
            {
                message = "The access token is invalid."
            });
        }

        var busCard = await _context.BusCards
            .AsNoTracking()
            .SingleOrDefaultAsync(card =>
                card.Id == cardId &&
                card.PassengerId == passengerId.Value);

        if (busCard is null)
        {
            return NotFound(new
            {
                message =
                    "The bus card was not found for this passenger."
            });
        }

        if (!busCard.TravelWalletId.HasValue)
        {
            return BadRequest(new
            {
                message =
                    "This bus card does not have an associated travel wallet."
            });
        }

        var transactions =
            await _context.WalletTransactions
                .AsNoTracking()
                .Where(transaction =>
                    transaction.TravelWalletId == busCard.TravelWalletId.Value)
                .OrderByDescending(transaction =>
                    transaction.CreatedAtUtc)
                .Select(transaction => new
                {
                    transaction.Id,
                    transaction.Reference,
                    transaction.TransactionType,
                    transaction.Amount,
                    transaction.BalanceAfter,
                    transaction.CreatedAtUtc,
                    transaction.BusCardId
                })
                .ToListAsync();

        return Ok(new
        {
            busCard,
            count = transactions.Count,
            transactions
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetMyWallet()
    {
        var passengerId = GetPassengerId();

        if (passengerId is null)
        {
            return Unauthorized(new
            {
                message = "The access token is invalid."
            });
        }

        var wallet = await _context.TravelWallets
            .AsNoTracking()
            .Where(wallet =>
                wallet.PassengerId == passengerId.Value)
            .Select(wallet => new
            {
                wallet.Id,
                wallet.Balance,
                wallet.Status,
                wallet.CreatedAtUtc,
                wallet.UpdatedAtUtc,

                cards = wallet.BusCards.Select(card => new
                {
                    card.Id,
                    card.CardNumber,
                    card.Status,
                    card.IsActive,
                    card.LinkedAtUtc
                })
            })
            .SingleOrDefaultAsync();

        if (wallet is null)
        {
            return NotFound(new
            {
                message = "Travel wallet not found."
            });
        }

        return Ok(wallet);
    }

    private int? GetPassengerId()
    {
        var passengerIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        return int.TryParse(
            passengerIdValue,
            out var passengerId)
            ? passengerId
            : null;
    }
}