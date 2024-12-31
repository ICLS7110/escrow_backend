using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace EscrowTestProject
{
    public class UserDetailsApiTests
    {
        private readonly HttpClient _httpClient;

        public UserDetailsApiTests()
        {
            _httpClient = new HttpClient();
        }

        [Fact]
        public async Task SaveUserDetails_WithValidToken_ShouldSaveSuccessfully()
        {
            // Step 1: Get the access token
            var tokenResponse = await GetAccessTokenAsync();
            Assert.False(string.IsNullOrEmpty(tokenResponse), "Failed to retrieve access token");

            // Step 2: Call the SaveUserDetails API
            var userDetails = new
            {
                FullName = "John Doe",
                EmailAddress = "john.doe@example.com",
                Gender = "Male",
                DateOfBirth = "1990-01-01",
                BusinessManagerName = "Jane Smith",
                BusinessEmail = "business@example.com",
                VatId = "VAT123456",
                ProofOfBusiness = (byte[])null, // Replace with actual byte array if needed
                AccountHolderName = "John Doe",
                IBANNumber = "DE89370400440532013000",
                BICCode = "COBADEFFXXX",
                LoginMethod = "Google"
            };

            var apiResponse = await SaveUserDetailsAsync(tokenResponse, userDetails);

            Assert.True(apiResponse.IsSuccessStatusCode, $"API call failed with status code {apiResponse.StatusCode}");
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var tokenUrl = "https://your-authority-url/connect/token"; // Replace with your authority URL
            var clientId = "your-client-id"; // Replace with your client ID
            var clientSecret = "your-client-secret"; // Replace with your client secret
            var username = "testuser"; // Replace with your test username
            var password = "testpassword"; // Replace with your test password

            var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
            {
                Content = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password)
            })
            };

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(responseString);

            if (jsonDocument.RootElement.TryGetProperty("access_token", out var token))
            {
                return token.GetString();
            }

            throw new Exception("Access token not found in response.");
        }

        private async Task<HttpResponseMessage> SaveUserDetailsAsync(string accessToken, object userDetails)
        {
            var apiUrl = "https://localhost:5001/api/UserDetails/SaveUserDetails";
            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(userDetails), Encoding.UTF8, "application/json")
            };

            // Add the bearer token to the Authorization header
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return await _httpClient.SendAsync(request);
        }
    }
}
