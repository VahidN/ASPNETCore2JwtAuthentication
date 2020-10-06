using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASPNETCore2JwtAuthentication.IntegrationTests
{
    [TestClass]
    public class JwtTests
    {
        [TestMethod]
        public async Task TestLoginWorks()
        {
            // Arrange
            var client = TestsHttpClient.Instance;

            // Act
            var token = await doLoginAsync(client);

            // Assert
            token.Should().NotBeNull();
            token.AccessToken.Should().NotBeNullOrEmpty();
            token.RefreshToken.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public async Task TestCallProtectedApiWorks()
        {
            // Arrange
            var client = TestsHttpClient.Instance;

            // Act
            var token = await doLoginAsync(client);

            // Assert
            token.Should().NotBeNull();
            token.AccessToken.Should().NotBeNullOrEmpty();
            token.RefreshToken.Should().NotBeNullOrEmpty();

            // Act
            const string protectedApiUrl = "/api/MyProtectedApi";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            var response = await client.GetAsync(protectedApiUrl);
            response.EnsureSuccessStatusCode();

            // Assert
            var responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().NotBeNullOrEmpty();
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var apiResponse = JsonSerializer.Deserialize<MyProtectedApiResponse>(responseString, options);
            apiResponse.Title.Should().NotBeNullOrEmpty();
            apiResponse.Title.Should().Be("Hello from My Protected Controller! [Authorize]");
        }

        private static async Task<Token> doLoginAsync(HttpClient client)
        {
            const string loginUrl = "/api/account/login";
            var user = new { Username = "Vahid", Password = "1234" };
            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, loginUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json")
            });
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().NotBeNullOrEmpty();
            return JsonSerializer.Deserialize<Token>(responseString);
        }
    }
}