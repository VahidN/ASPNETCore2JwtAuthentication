using System.Text;
using ASPNETCore2JwtAuthentication.IntegrationTests.Base;
using ASPNETCore2JwtAuthentication.IntegrationTests.Models;
using ASPNETCore2JwtAuthentication.Models;
using ASPNETCore2JwtAuthentication.WebApp.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Token = ASPNETCore2JwtAuthentication.IntegrationTests.Models.Token;

namespace ASPNETCore2JwtAuthentication.IntegrationTests;

[TestClass]
public class JwtTests
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [TestMethod]
    public async Task TestLoginWorks()
    {
        // Arrange
        var client = TestsHttpClient.Instance;

        // Act
        var token = await doLoginAsync(client.HttpClient, client.LinkGenerator, client.AdminUserSeed);

        // Assert
        token.Should().NotBeNull();
        token?.AccessToken.Should().NotBeNullOrEmpty();
        token?.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [TestMethod]
    public async Task TestCallProtectedApiWorks()
    {
        // Arrange
        var client = TestsHttpClient.Instance;

        // Act
        var token = await doLoginAsync(client.HttpClient, client.LinkGenerator, client.AdminUserSeed);

        // Assert
        token.Should().NotBeNull();

        if (token is null)
        {
            return;
        }

        token.AccessToken.Should().NotBeNullOrEmpty();
        token.RefreshToken.Should().NotBeNullOrEmpty();

        // Act
        var protectedApiUrl = client.LinkGenerator.GetPathByAction(nameof(MyProtectedApiController.Get),
            nameof(MyProtectedApiController).Replace(oldValue: "Controller", newValue: "", StringComparison.Ordinal));

        client.HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(scheme: "Bearer", token.AccessToken);

        var response = await client.HttpClient.GetAsync(protectedApiUrl);
        response.EnsureSuccessStatusCode();

        // Assert
        var responseString = await response.Content.ReadAsStringAsync();
        responseString.Should().NotBeNullOrEmpty();
        var apiResponse = JsonSerializer.Deserialize<MyProtectedApiResponse>(responseString, JsonSerializerOptions);
        apiResponse.Should().NotBeNull();
        apiResponse?.Title.Should().NotBeNullOrEmpty();
        apiResponse?.Title.Should().Be(expected: "Hello from My Protected Controller! [Authorize]");
    }

    private static async Task<Token?> doLoginAsync(HttpClient client,
        LinkGenerator linkGenerator,
        AdminUserSeed adminUserSeed)
    {
        ArgumentNullException.ThrowIfNull(client);

        var loginUrl = linkGenerator.GetPathByAction(nameof(AccountController.Login),
            nameof(AccountController).Replace(oldValue: "Controller", newValue: "", StringComparison.Ordinal));

        var user = new
        {
            adminUserSeed.Username,
            adminUserSeed.Password
        };

        using var stringContent =
            new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, mediaType: "application/json");

        using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, loginUrl)
        {
            Content = stringContent
        };

        var response = await client.SendAsync(httpRequestMessage);
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        responseString.Should().NotBeNullOrEmpty();

        return JsonSerializer.Deserialize<Token>(responseString);
    }
}