using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TshwaneMetroRide.Api.Data;
using TshwaneMetroRide.Api.DTOs.Routes;

namespace TshwaneMetroRide.Api.Controllers;

[ApiController]
[Route("api/routes")]
public class RoutesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public RoutesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RouteResponse>>>
        GetAllRoutes()
    {
        var routes = await _context.BusRoutes
            .AsNoTracking()
            .Where(route => route.IsActive)
            .OrderBy(route => route.RouteName)
            .Select(route => new RouteResponse(
                route.Id,
                route.RouteCode,
                route.RouteName,
                route.Origin,
                route.Destination,
                route.Stops,
                route.FareAmount,
                route.IsActive))
            .ToListAsync();

        return Ok(new
        {
            count = routes.Count,
            routes
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RouteResponse>>
        GetRouteById(int id)
    {
        var route = await _context.BusRoutes
            .AsNoTracking()
            .Where(route => route.Id == id)
            .Select(route => new RouteResponse(
                route.Id,
                route.RouteCode,
                route.RouteName,
                route.Origin,
                route.Destination,
                route.Stops,
                route.FareAmount,
                route.IsActive))
            .SingleOrDefaultAsync();

        if (route is null)
        {
            return NotFound(new
            {
                message = "The requested bus route was not found."
            });
        }

        return Ok(route);
    }

    [HttpGet("code/{routeCode}")]
    public async Task<ActionResult<RouteResponse>>
        GetRouteByCode(string routeCode)
    {
        var normalizedRouteCode = routeCode
            .Trim()
            .ToUpperInvariant();

        var route = await _context.BusRoutes
            .AsNoTracking()
            .Where(route =>
                route.RouteCode == normalizedRouteCode)
            .Select(route => new RouteResponse(
                route.Id,
                route.RouteCode,
                route.RouteName,
                route.Origin,
                route.Destination,
                route.Stops,
                route.FareAmount,
                route.IsActive))
            .SingleOrDefaultAsync();

        if (route is null)
        {
            return NotFound(new
            {
                message = "The requested bus route was not found."
            });
        }

        return Ok(route);
    }
}