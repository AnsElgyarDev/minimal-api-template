using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyCompany.MinimalApi.Data;
using MyCompany.MinimalApi.Models;
using MyCompany.MinimalApi.Services;

namespace MyCompany.MinimalApi.Endpoints;

public record RegisterRequest(string? Email, string? Password);
public record LoginRequest(string? Email, string? Password);

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/register", Register);
        group.MapPost("/login", Login);

        return app;
    }

    private static async Task<IResult> Register(
        RegisterRequest request,
        AppDbContext db,
        IPasswordHasher<User> hasher)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
            errors["email"] = ["A valid email is required."];
        if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 8)
            errors["password"] = ["Password must be at least 8 characters."];
        if (errors.Count > 0)
            return Results.ValidationProblem(errors);

        var email = request.Email!.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(u => u.Email == email))
            return Results.Problem(statusCode: StatusCodes.Status409Conflict, title: "Email is already registered.");

        var user = new User { Email = email };
        user.PasswordHash = hasher.HashPassword(user, request.Password!);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return Results.Ok(new { user.Id, user.Email });
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        AppDbContext db,
        IPasswordHasher<User> hasher,
        ITokenService tokens)
    {
        var email = (request.Email ?? string.Empty).Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

        // Same message for unknown email and wrong password, so accounts can't be probed.
        if (user is null ||
            hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password ?? string.Empty)
                == PasswordVerificationResult.Failed)
        {
            return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid email or password.");
        }

        var (token, expiresAt) = tokens.CreateToken(user);
        return Results.Ok(new { token, expiresAt });
    }
}