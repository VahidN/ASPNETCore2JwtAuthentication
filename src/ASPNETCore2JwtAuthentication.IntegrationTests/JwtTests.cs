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
        var protectedApiUrl = client.LinkGenerator
                                    .GetPathByAction(nameof(MyProtectedApiController.Get),
                                                     nameof(MyProtectedApiController).Replace(
                                                      "Controller", "", StringComparison.Ordinal));
        client.HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.AccessToken);
        var response = await client.HttpClient.GetAsync(protectedApiUrl);
        response.EnsureSuccessStatusCode();

        // Assert
        var responseString = await response.Content.ReadAsStringAsync();
        responseString.Should().NotBeNullOrEmpty();
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var apiResponse = JsonSerializer.Deserialize<MyProtectedApiResponse>(responseString, options);
        apiResponse.Should().NotBeNull();
        apiResponse?.Title.Should().NotBeNullOrEmpty();
        apiResponse?.Title.Should().Be("Hello from My Protected Controller! [Authorize]");
    }

    private static async Task<Token?> doLoginAsync(
        HttpClient client,
        LinkGenerator linkGenerator,
        AdminUserSeed adminUserSeed)
    {
        if (client is null)
        {
            throw new ArgumentNullException(nameof(client));
        }

        var loginUrl = linkGenerator.GetPathByAction(nameof(AccountController.Login),
                                                     nameof(AccountController)
                                                         .Replace("Controller", "", StringComparison.Ordinal));
        var user = new { adminUserSeed.Username, adminUserSeed.Password };
        using var stringContent = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");
        using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, loginUrl)
                                       {
                                           Content = stringContent,
                                       };
        var response = await client.SendAsync(httpRequestMessage);
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        responseString.Should().NotBeNullOrEmpty();
        return JsonSerializer.Deserialize<Token>(responseString);
    }
}