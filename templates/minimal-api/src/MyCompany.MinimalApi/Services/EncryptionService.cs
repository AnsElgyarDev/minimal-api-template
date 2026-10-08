using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace MyCompany.MinimalApi.Services;

/// <summary>
/// AES-256-GCM. Output (Base64) layout: nonce (12 bytes) | tag (16 bytes) | cipher text.
/// A new random nonce is generated per call, so the same text never encrypts to the same output.
/// </summary>
public sealed class EncryptionService : IEncryptionService
{
    private const int NonceSize = 12;
    private const int TagSize = 16;

    private readonly byte[] _key;

    public EncryptionService(IOptions<EncryptionSettings> options)
    {
        var settings = options.Value;

        if (!settings.IsKeyValid())
        {
            throw new InvalidOperationException(
                $"{EncryptionSettings.SectionName}:Key must be a Base64 string of exactly " +
                $"{EncryptionSettings.KeySizeInBytes} bytes.");
        }

        _key = Convert.FromBase64String(settings.Key);
    }

    public string Encrypt(string plainText)
    {
        ArgumentNullException.ThrowIfNull(plainText);

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var cipher = new byte[plainBytes.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plainBytes, cipher, tag);

        var result = new byte[NonceSize + TagSize + cipher.Length];
        nonce.CopyTo(result, 0);
        tag.CopyTo(result, NonceSize);
        cipher.CopyTo(result, NonceSize + TagSize);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        ArgumentNullException.ThrowIfNull(cipherText);

        byte[] data;
        try
        {
            data = Convert.FromBase64String(cipherText);
        }
        catch (FormatException ex)
        {
            throw new CryptographicException("Invalid cipher text.", ex);
        }

        if (data.Length < NonceSize + TagSize)
        {
            throw new CryptographicException("Invalid cipher text.");
        }

        var nonce = data.AsSpan(0, NonceSize);
        var tag = data.AsSpan(NonceSize, TagSize);
        var cipher = data.AsSpan(NonceSize + TagSize);
        var plainBytes = new byte[cipher.Length];

        // Throws a CryptographicException if the data was tampered with or the key is wrong.
        using var aes = new AesGcm(_key, TagSize);
        aes.Decrypt(nonce, cipher, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }
}