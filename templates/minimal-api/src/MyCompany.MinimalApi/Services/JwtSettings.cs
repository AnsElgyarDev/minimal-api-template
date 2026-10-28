namespace MyCompany.MinimalApi.Services;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";
    public const int MinKeyLength = 32;

    public string Key { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int ExpiryMinutes { get; init; } = 60;

    public bool IsValid() =>
        Key.Length >= MinKeyLength &&
        !string.IsNullOrWhiteSpace(Issuer) &&
        !string.IsNullOrWhiteSpace(Audience);
}