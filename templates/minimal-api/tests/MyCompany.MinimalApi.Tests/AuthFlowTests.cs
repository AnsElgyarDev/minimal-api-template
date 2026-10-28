using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MyCompany.MinimalApi.Tests;

public class AuthFlowTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    public record TokenResponse(string Token, DateTime ExpiresAt);

    private static readonly object Credentials = new { email = "user@example.com", password = "Passw0rd!123" };

    private async Task<HttpClient> LoggedInClientAsync()
    {
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/auth/register", Credentials);

        var login = await client.PostAsJsonAsync("/auth/login", Credentials);
        var body = await login.Content.ReadFromJsonAsync<TokenResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.Token);
        return client;
    }

    [Fact]
    public async Task Register_login_and_read_own_profile()
    {
        var client = await LoggedInClientAsync();

        var me = await client.GetAsync("/users/me");

        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
    }

    [Fact]
    public async Task Me_without_token_returns_401()
    {
        var response = await factory.CreateClient().GetAsync("/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Normal_user_cannot_list_users()
    {
        var client = await LoggedInClientAsync();

        var response = await client.GetAsync("/users");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}