using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Escrow.Api.Infrastructure.Services;
public class AnbService : IAnbService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ANBSettings _settings;
    private readonly ILogger<AnbService> _logger;

    public AnbService(HttpClient httpClient, IConfiguration configuration, IOptions<ANBSettings> settings, ILogger<AnbService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _settings = settings.Value; // ✅ Fix the type conversion
        _logger = logger;
    }



    public class AnbTokenResponse
    {
        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("scope")]
        public string? Scope { get; set; }

        [JsonPropertyName("issued_at")]
        public long IssuedAt { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token_expires_in")]
        public int RefreshTokenExpiresIn { get; set; }

        [JsonPropertyName("user")]
        public AnbTokenUser? User { get; set; }
    }
    public class AnbTokenUser
    {
        [JsonPropertyName("accounts")]
        public string? Accounts { get; set; }
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        _logger.LogInformation("Starting GetAccessTokenAsync...");

        if (string.IsNullOrWhiteSpace(_settings.BaseUrl) ||
            string.IsNullOrWhiteSpace(_settings.TokenEndpoint) ||
            string.IsNullOrWhiteSpace(_settings.ClientId) ||
            string.IsNullOrWhiteSpace(_settings.ClientSecret))
        {
            _logger.LogError("ANB settings are not properly configured.");
            throw new InvalidOperationException("ANB settings are not properly configured.");
        }

        var requestUrl = $"{_settings.BaseUrl.TrimEnd('/')}/{_settings.TokenEndpoint.TrimStart('/')}";
        _logger.LogDebug("Token request URL: {RequestUrl}", requestUrl);

        var encodedGrantType = Uri.EscapeDataString("client_credentials");
        var encodedClientId = Uri.EscapeDataString(_settings.ClientId);
        var encodedClientSecret = Uri.EscapeDataString(_settings.ClientSecret);

        // Construct and log a real curl command for debugging purposes
        var curlCommand = $"curl --location --request POST '{requestUrl}' " +
                          $"--header 'Content-Type: application/x-www-form-urlencoded' " +
                          $"--data-urlencode 'grant_type={encodedGrantType}' " +
                          $"--data-urlencode 'client_id={encodedClientId}' " +
                          $"--data-urlencode 'client_secret={encodedClientSecret}'";

        _logger.LogInformation("Equivalent curl command:\n{CurlCommand}", curlCommand);

        var formBody = $"grant_type={encodedGrantType}&client_id={encodedClientId}&client_secret={encodedClientSecret}";

        var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = new StringContent(formBody, Encoding.UTF8, "application/x-www-form-urlencoded");

        _logger.LogDebug("Request body: {Body}", formBody);

        HttpResponseMessage response;

        try
        {
            response = await _httpClient.SendAsync(request);
            _logger.LogInformation("Token request sent. Status Code: {StatusCode}", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HTTP request to get access token failed.");
            throw;
        }

        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Token request failed with status {StatusCode}: {ErrorBody}", response.StatusCode, responseContent);
            throw new HttpRequestException($"Token request failed: {response.StatusCode} - {responseContent}");
        }

        try
        {
            var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<AnbTokenResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (tokenResponse?.AccessToken == null)
            {
                _logger.LogWarning("Access token is null in the token response.");
            }
            else
            {
                _logger.LogInformation("Access token retrieved successfully.");
            }

            return tokenResponse?.AccessToken;
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize token response: {ResponseContent}", responseContent);
            throw;
        }
    }
    public async Task<VerifyAccountResponse?> VerifyAccountAsync(VerifyAccountRequest request)
    {
        if (string.IsNullOrWhiteSpace(_settings.BaseUrl))
        {
            _logger.LogError("ANB base URL is not configured.");
            throw new InvalidOperationException("ANB base URL is not configured.");
        }

        var accessToken = await GetAccessTokenAsync(); // Get from OAuth
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            _logger.LogError("Failed to retrieve access token.");
            return null;
        }

        var requestUrl = $"{_settings.BaseUrl.TrimEnd('/')}/avs/account-verification";
        var requestBody = new
        {
            iban = request.Iban,
            nationalId = request.NationalId,
            destinationBankBIC = request.DestinationBankBIC
        };

        var jsonBody = JsonConvert.SerializeObject(requestBody);
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl)
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
        };

        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        requestMessage.Headers.Add("Cookie", _settings.AnbCookie ?? "");

        var curlCommand = $"curl --location '{requestUrl}' \\\n" +
                          $"--header 'accept: application/json' \\\n" +
                          $"--header 'Content-Type: application/json' \\\n" +
                          $"--header 'Authorization: Bearer {accessToken}' \\\n" +
                          $"--header 'Cookie: {_settings.AnbCookie}' \\\n" +
                          $"--data '{jsonBody}'";

        _logger.LogInformation("ANB curl command:\n{Curl}", curlCommand);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(requestMessage);
            _logger.LogInformation("ANB response received. Status Code: {StatusCode}", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HTTP request to ANB failed.");
            return null;
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        _logger.LogDebug("Raw response content: {Response}", responseContent);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Account verification failed. Status: {StatusCode}, Body: {Body}", response.StatusCode, responseContent);
            return null;
        }

        try
        {
            var result = JsonConvert.DeserializeObject<VerifyAccountResponse>(responseContent);
            _logger.LogInformation("Account verification succeeded. Status: {Status}", result?.Status);
            return result;
        }
        catch (Newtonsoft.Json.JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize ANB response.");
            return new VerifyAccountResponse
            {
                RequestReferenceNumber = ex.Message,
                Status = "FAILED",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

        }
    }
    public async Task<string> GetAccountVerificationStatusAsync(string requestReferenceNumber)
    {
        try
        {
            var token = await GetAccessTokenAsync();
            var baseUrl = _configuration["AnbConnect:BaseUrl"]
                ?? throw new InvalidOperationException("ANB base URL is missing in configuration.");

            var url = $"{baseUrl.TrimEnd('/')}/avs/account-verification/{requestReferenceNumber}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var cookie = _configuration["AnbConnect:Cookie"];
            if (!string.IsNullOrWhiteSpace(cookie))
            {
                request.Headers.Add("Cookie", cookie);
            }

            // Build cURL command string for logging
            var curlBuilder = new StringBuilder();
            curlBuilder.AppendLine($"curl --location '{url}' \\");
            curlBuilder.AppendLine("--header 'accept: application/json' \\");
            curlBuilder.AppendLine("--header 'Content-Type: application/json' \\");
            curlBuilder.AppendLine($"--header 'Authorization: Bearer {token}' \\");
            if (!string.IsNullOrWhiteSpace(cookie))
                curlBuilder.AppendLine($"--header 'Cookie: {cookie}' \\");

            _logger.LogInformation("ANB GetAccountVerificationStatusAsync cURL command:\n{CurlCommand}", curlBuilder.ToString());

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("ANB GetAccountVerificationStatusAsync response status: {StatusCode}", response.StatusCode);
            _logger.LogDebug("ANB GetAccountVerificationStatusAsync raw response content: {Content}", content);

            response.EnsureSuccessStatusCode();  // Throw if not success

            return content;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching account verification for reference {ReferenceNumber}", requestReferenceNumber);
            throw new ApplicationException($"HTTP error while fetching account verification for reference {requestReferenceNumber}: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timed out while fetching account verification for reference {ReferenceNumber}", requestReferenceNumber);
            throw new ApplicationException($"Request timed out while fetching account verification for reference {requestReferenceNumber}.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching account verification for reference {ReferenceNumber}", requestReferenceNumber);
            throw new ApplicationException($"Unexpected error while fetching account verification for reference {requestReferenceNumber}: {ex.Message}", ex);
        }
    }
}
