using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TshwaneMetroRide.Api.Data;
using TshwaneMetroRide.Api.DTOs.Buses;
using TshwaneMetroRide.Api.Models;

namespace TshwaneMetroRide.Api.Controllers;

[ApiController]
[Route("api/buses")]
public class BusesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BusesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetBuses(
        int? routeId,
        string? status)
    {
        var query = _context.Buses
            .AsNoTracking()
            .AsQueryable();

        if (routeId.HasValue)
        {
            query = query.Where(bus =>
                bus.BusRouteId == routeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus =
                NormalizeStatus(status);

            if (normalizedStatus is null)
            {
                return BadRequest(new
                {
                    message =
                        "Status must be Active, Maintenance, or OutOfService."
                });
            }

            query = query.Where(bus =>
                bus.Status == normalizedStatus);
        }

        var buses = await query
            .OrderBy(bus => bus.FleetNumber)
            .Select(bus => new
            {
                bus.Id,
                bus.FleetNumber,
                bus.RegistrationNumber,
                bus.Capacity,
                bus.Status,
                bus.CreatedAtUtc,
                bus.UpdatedAtUtc,

                route = bus.BusRoute == null
                    ? null
                    : new
                    {
                        bus.BusRoute.Id,
                        bus.BusRoute.RouteCode,
                        bus.BusRoute.RouteName,
                        bus.BusRoute.Origin,
                        bus.BusRoute.Destination
                    }
            })
            .ToListAsync();

        return Ok(new
        {
            count = buses.Count,
            buses
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetBusById(int id)
    {
        var bus = await _context.Buses
            .AsNoTracking()
            .Where(bus => bus.Id == id)
            .Select(bus => new
            {
                bus.Id,
                bus.FleetNumber,
                bus.RegistrationNumber,
                bus.Capacity,
                bus.Status,
                bus.CreatedAtUtc,
                bus.UpdatedAtUtc,

                route = bus.BusRoute == null
                    ? null
                    : new
                    {
                        bus.BusRoute.Id,
                        bus.BusRoute.RouteCode,
                        bus.BusRoute.RouteName,
                        bus.BusRoute.Origin,
                        bus.BusRoute.Destination,
                        bus.BusRoute.Stops,
                        bus.BusRoute.FareAmount
                    }
            })
            .SingleOrDefaultAsync();

        if (bus is null)
        {
            return NotFound(new
            {
                message = "The requested bus was not found."
            });
        }

        return Ok(bus);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateBus(
        CreateBusRequest request)
    {
        var fleetNumber = request.FleetNumber
            .Trim()
            .ToUpperInvariant();

        var registrationNumber = request.RegistrationNumber
            .Trim()
            .ToUpperInvariant();

        var fleetExists = await _context.Buses
            .AnyAsync(bus =>
                bus.FleetNumber == fleetNumber);

        if (fleetExists)
        {
            return Conflict(new
            {
                message =
                    "A bus with this fleet number already exists."
            });
        }

        var registrationExists = await _context.Buses
            .AnyAsync(bus =>
                bus.RegistrationNumber ==
                registrationNumber);

        if (registrationExists)
        {
            return Conflict(new
            {
                message =
                    "A bus with this registration number already exists."
            });
        }

        if (request.BusRouteId.HasValue)
        {
            var routeExists = await _context.BusRoutes
                .AnyAsync(route =>
                    route.Id == request.BusRouteId.Value &&
                    route.IsActive);

            if (!routeExists)
            {
                return BadRequest(new
                {
                    message =
                        "The selected route does not exist or is inactive."
                });
            }
        }

        var bus = new Bus
        {
            FleetNumber = fleetNumber,
            RegistrationNumber = registrationNumber,
            Capacity = request.Capacity,
            Status = "Active",
            BusRouteId = request.BusRouteId,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Buses.Add(bus);
        await _context.SaveChangesAsync();

        return Created(
            $"/api/buses/{bus.Id}",
            new
            {
                message = "Bus created successfully.",

                bus = new
                {
                    bus.Id,
                    bus.FleetNumber,
                    bus.RegistrationNumber,
                    bus.Capacity,
                    bus.Status,
                    bus.BusRouteId,
                    bus.CreatedAtUtc
                }
            });
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateBus(
        int id,
        UpdateBusRequest request)
    {
        var bus = await _context.Buses
            .SingleOrDefaultAsync(bus => bus.Id == id);

        if (bus is null)
        {
            return NotFound(new
            {
                message = "The requested bus was not found."
            });
        }

        var fleetNumber = request.FleetNumber
            .Trim()
            .ToUpperInvariant();

        var registrationNumber = request.RegistrationNumber
            .Trim()
            .ToUpperInvariant();

        var fleetExists = await _context.Buses
            .AnyAsync(otherBus =>
                otherBus.Id != id &&
                otherBus.FleetNumber == fleetNumber);

        if (fleetExists)
        {
            return Conflict(new
            {
                message =
                    "Another bus already uses this fleet number."
            });
        }

        var registrationExists = await _context.Buses
            .AnyAsync(otherBus =>
                otherBus.Id != id &&
                otherBus.RegistrationNumber ==
                registrationNumber);

        if (registrationExists)
        {
            return Conflict(new
            {
                message =
                    "Another bus already uses this registration number."
            });
        }

        if (request.BusRouteId.HasValue)
        {
            var routeExists = await _context.BusRoutes
                .AnyAsync(route =>
                    route.Id == request.BusRouteId.Value &&
                    route.IsActive);

            if (!routeExists)
            {
                return BadRequest(new
                {
                    message =
                        "The selected route does not exist or is inactive."
                });
            }
        }

        bus.FleetNumber = fleetNumber;
        bus.RegistrationNumber = registrationNumber;
        bus.Capacity = request.Capacity;
        bus.BusRouteId = request.BusRouteId;
        bus.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Bus updated successfully.",

            bus = new
            {
                bus.Id,
                bus.FleetNumber,
                bus.RegistrationNumber,
                bus.Capacity,
                bus.Status,
                bus.BusRouteId,
                bus.UpdatedAtUtc
            }
        });
    }

    [HttpPatch("{id:int}/status")]
    [Authorize]
    public async Task<IActionResult> ChangeBusStatus(
        int id,
        ChangeBusStatusRequest request)
    {
        var normalizedStatus =
            NormalizeStatus(request.Status);

        if (normalizedStatus is null)
        {
            return BadRequest(new
            {
                message =
                    "Status must be Active, Maintenance, or OutOfService."
            });
        }

        var bus = await _context.Buses
            .SingleOrDefaultAsync(bus => bus.Id == id);

        if (bus is null)
        {
            return NotFound(new
            {
                message = "The requested bus was not found."
            });
        }

        if (bus.Status == normalizedStatus)
        {
            return BadRequest(new
            {
                message =
                    $"The bus is already marked as {normalizedStatus}."
            });
        }

        bus.Status = normalizedStatus;
        bus.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Bus status updated successfully.",

            bus = new
            {
                bus.Id,
                bus.FleetNumber,
                bus.RegistrationNumber,
                bus.Status,
                bus.UpdatedAtUtc
            }
        });
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
            "active" => "Active",
            "maintenance" => "Maintenance",
            "outofservice" => "OutOfService",
            _ => null
        };
    }
}