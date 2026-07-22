using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TshwaneMetroRide.Api.Data;
using TshwaneMetroRide.Api.DTOs.Support;
using TshwaneMetroRide.Api.Models;

namespace TshwaneMetroRide.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/support")]
public class SupportController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SupportController(
        ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateRequest(
        CreateSupportRequest request)
    {
        var passengerId = GetPassengerId();

        if (passengerId is null)
        {
            return Unauthorized(new
            {
                message = "The access token is invalid."
            });
        }

        var passengerExists = await _context.Passengers
            .AnyAsync(passenger =>
                passenger.Id == passengerId.Value);

        if (!passengerExists)
        {
            return NotFound(new
            {
                message = "Passenger account not found."
            });
        }

        var supportRequest = new SupportRequest
        {
            Subject = request.Subject.Trim(),
            Message = request.Message.Trim(),
            Status = "Open",
            CreatedAtUtc = DateTime.UtcNow,
            PassengerId = passengerId.Value
        };

        _context.SupportRequests.Add(supportRequest);
        await _context.SaveChangesAsync();

        return Created(
            $"/api/support/{supportRequest.Id}",
            new
            {
                message =
                    "Support request submitted successfully.",

                supportRequest = new
                {
                    supportRequest.Id,
                    supportRequest.Subject,
                    supportRequest.Message,
                    supportRequest.Status,
                    supportRequest.CreatedAtUtc
                }
            });
    }

    [HttpGet]
    public async Task<IActionResult> GetMyRequests(
        string? status)
    {
        var passengerId = GetPassengerId();

        if (passengerId is null)
        {
            return Unauthorized(new
            {
                message = "The access token is invalid."
            });
        }

        var query = _context.SupportRequests
            .AsNoTracking()
            .Where(request =>
                request.PassengerId ==
                passengerId.Value);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus =
                NormalizeStatus(status);

            if (normalizedStatus is null)
            {
                return BadRequest(new
                {
                    message =
                        "Status must be Open, InProgress, Resolved, or Closed."
                });
            }

            query = query.Where(request =>
                request.Status == normalizedStatus);
        }

        var requests = await query
            .OrderByDescending(request =>
                request.CreatedAtUtc)
            .Select(request => new
            {
                request.Id,
                request.Subject,
                request.Message,
                request.Status,
                request.CreatedAtUtc,
                request.UpdatedAtUtc,
                request.ResolvedAtUtc
            })
            .ToListAsync();

        return Ok(new
        {
            count = requests.Count,
            supportRequests = requests
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetRequestById(
        int id)
    {
        var passengerId = GetPassengerId();

        if (passengerId is null)
        {
            return Unauthorized(new
            {
                message = "The access token is invalid."
            });
        }

        var supportRequest =
            await _context.SupportRequests
                .AsNoTracking()
                .Where(request =>
                    request.Id == id &&
                    request.PassengerId ==
                    passengerId.Value)
                .Select(request => new
                {
                    request.Id,
                    request.Subject,
                    request.Message,
                    request.Status,
                    request.CreatedAtUtc,
                    request.UpdatedAtUtc,
                    request.ResolvedAtUtc
                })
                .SingleOrDefaultAsync();

        if (supportRequest is null)
        {
            return NotFound(new
            {
                message =
                    "The support request was not found."
            });
        }

        return Ok(supportRequest);
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

    private static string? NormalizeStatus(
        string status)
    {
        var normalized = status
            .Trim()
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty)
            .ToLowerInvariant();

        return normalized switch
        {
            "open" => "Open",
            "inprogress" => "InProgress",
            "resolved" => "Resolved",
            "closed" => "Closed",
            _ => null
        };
    }
}