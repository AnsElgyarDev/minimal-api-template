using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyCompany.MinimalApi.Services;

namespace MyCompany.MinimalApi.Helpers;

public static class JwtAuthExtensions
{
    public static IServiceCollection AddJwtAuth(this IServiceCollection services)
    {
        services.AddOptions<JwtSettings>()
            .BindConfiguration(JwtSettings.SectionName)
            .Validate(s => s.IsValid(),
                "Jwt:Key must be at least 32 characters, and Jwt:Issuer and Jwt:Audience must be set. " +
                "Set the key using user-secrets or an environment variable (Jwt__Key).")
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtSettings>>((options, jwt) =>
            {
                var s = jwt.Value;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = s.Issuer,
                    ValidateAudience = true,
                    ValidAudience = s.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero, // removes the default 5 minutes tolerance
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(s.Key))
                };
            });

        return services;
    }
}