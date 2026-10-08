using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using MyCompany.MinimalApi.Services;

namespace MyCompany.MinimalApi.Tests;

public class EncryptionServiceTests
{
    private static string NewKey() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(EncryptionSettings.KeySizeInBytes));

    private static EncryptionService CreateService(string? key = null) =>
        new(Options.Create(new EncryptionSettings { Key = key ?? NewKey() }));

    [Fact]
    public void Decrypt_ReturnsOriginalText()
    {
        var service = CreateService();

        var cipher = service.Encrypt("hello world");

        Assert.Equal("hello world", service.Decrypt(cipher));
    }

    [Fact]
    public void Encrypt_SameText_ProducesDifferentOutput()
    {
        var service = CreateService();

        Assert.NotEqual(service.Encrypt("same"), service.Encrypt("same"));
    }

    [Fact]
    public void Decrypt_TamperedData_Throws()
    {
        var service = CreateService();
        var data = Convert.FromBase64String(service.Encrypt("secret"));
        data[^1] ^= 0xFF;

        Assert.ThrowsAny<CryptographicException>(() => service.Decrypt(Convert.ToBase64String(data)));
    }

    [Fact]
    public void Decrypt_WithDifferentKey_Throws()
    {
        var cipher = CreateService().Encrypt("secret");

        Assert.ThrowsAny<CryptographicException>(() => CreateService().Decrypt(cipher));
    }

    [Theory]
    [InlineData("")]
    [InlineData("SET_VIA_USER_SECRETS")]
    [InlineData("not-base64!")]
    [InlineData("c2hvcnQ=")] // valid Base64 but only 5 bytes
    public void Constructor_InvalidKey_Throws(string key)
    {
        Assert.Throws<InvalidOperationException>(() => CreateService(key));
    }
}