namespace MyCompany.MinimalApi.Helpers;

public static class Policies
{
    public const string AdminOnly = "AdminOnly";
    public const string AuthenticatedUser = "AuthenticatedUser";
}

public static class AppPoliciesExtensions
{
    // Registers authorization and the app policies.
    // RequireRole works only if the JWT contains role claims (ClaimTypes.Role).
    public static IServiceCollection AddAppPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.AdminOnly, policy => policy.RequireRole("Admin"))
            .AddPolicy(Policies.AuthenticatedUser, policy => policy.RequireAuthenticatedUser());

        return services;
    }
}