using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MyCompany.MinimalApi.Data;
using MyCompany.MinimalApi.Helpers;

namespace MyCompany.MinimalApi.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users").WithTags("Users");

        group.MapGet("/me", async (ClaimsPrincipal principal, AppDbContext db) =>
        {
            var idValue = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(idValue, out var userId))
                return Results.Unauthorized();

            var user = await db.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new { u.Id, u.Email, u.Role })
                .FirstOrDefaultAsync();

            return user is null ? Results.NotFound() : Results.Ok(user);
        })
        .RequireAuthorization(Policies.AuthenticatedUser);

        group.MapGet("/", async (AppDbContext db) =>
            Results.Ok(await db.Users
                .AsNoTracking()
                .Select(u => new { u.Id, u.Email, u.Role })
                .ToListAsync()))
        .RequireAuthorization(Policies.AdminOnly);

        return app;
    }
}