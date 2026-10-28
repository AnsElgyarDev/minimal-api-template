using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyCompany.MinimalApi.Data;
using MyCompany.MinimalApi.Endpoints;
using MyCompany.MinimalApi.Helpers;
using MyCompany.MinimalApi.Middlewares;
using MyCompany.MinimalApi.Models;
using MyCompany.MinimalApi.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddMemoryCache();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddAppPolicies(); // registers authorization + policies
builder.Services.AddJwtAuth();     // JWT bearer authentication + validated Jwt settings

builder.Services.AddOptions<EncryptionSettings>()
    .Bind(builder.Configuration.GetSection(EncryptionSettings.SectionName))
    .Validate(s => s.IsKeyValid(),
        "EncryptionSettings:Key must be Base64 of exactly 32 bytes. Set it using user-secrets or an environment variable.")
    .ValidateOnStart();
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();

builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<ITokenService, TokenService>();

var app = builder.Build();

// Logging first (so it sees the final status), then error handling and HTTPS
app.UseMiddleware<RequestLogMiddleware>();
app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapAuthEndpoints();
app.MapUserEndpoints();

app.Run();