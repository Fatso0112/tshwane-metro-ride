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

        if (!busCard.IsActive)
        {
            return BadRequest(new
            {
                message =
                    "This bus card is inactive and cannot be topped up."
            });
        }

        busCard.Balance += amount;

        var reference =
            $"TMR-{DateTime.UtcNow:yyyyMMddHHmmss}-" +
            $"{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

        var walletTransaction = new WalletTransaction
        {
            Amount = amount,
            TransactionType = "TopUp",
            Reference = reference,
            BalanceAfter = busCard.Balance,
            PaymentMethod = request.PaymentMethod,
            CreatedAtUtc = DateTime.UtcNow,
            BusCardId = busCard.Id
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
                walletTransaction.PaymentMethod,
                walletTransaction.CreatedAtUtc
            },

            busCard = new
            {
                busCard.Id,
                busCard.CardNumber,
                busCard.Balance
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
            .Where(card =>
                card.Id == cardId &&
                card.PassengerId == passengerId.Value)
            .Select(card => new
            {
                card.Id,
                card.CardNumber,
                card.Balance
            })
            .SingleOrDefaultAsync();

        if (busCard is null)
        {
            return NotFound(new
            {
                message =
                    "The bus card was not found for this passenger."
            });
        }

        var transactions =
            await _context.WalletTransactions
                .AsNoTracking()
                .Where(transaction =>
                    transaction.BusCardId == cardId)
                .OrderByDescending(transaction =>
                    transaction.CreatedAtUtc)
                .Select(transaction => new
                {
                    transaction.Id,
                    transaction.Reference,
                    transaction.TransactionType,
                    transaction.Amount,
                    transaction.BalanceAfter,
                    transaction.CreatedAtUtc
                })
                .ToListAsync();

        return Ok(new
        {
            busCard,
            count = transactions.Count,
            transactions
        });
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