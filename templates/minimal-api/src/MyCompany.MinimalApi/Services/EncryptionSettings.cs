namespace MyCompany.MinimalApi.Services;

public sealed class EncryptionSettings
{
    public const string SectionName = "EncryptionSettings";
    public const string KeyPlaceholder = "SET_VIA_USER_SECRETS";
    public const int KeySizeInBytes = 32; // AES-256

    // Base64 text that decodes to exactly 32 bytes.
    public string Key { get; init; } = string.Empty;

    public bool IsKeyValid()
    {
        if (string.IsNullOrWhiteSpace(Key) || Key == KeyPlaceholder)
        {
            return false;
        }

        try
        {
            return Convert.FromBase64String(Key).Length == KeySizeInBytes;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}