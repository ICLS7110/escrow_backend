using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Escrow.Api.Infrastructure.Services;
public class AnbService : IAnbService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ANBSettings _settings;

    public AnbService(HttpClient httpClient, IConfiguration configuration, IOptions<ANBSettings> settings)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _settings = settings.Value; // ✅ Fix the type conversion
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        if (string.IsNullOrWhiteSpace(_settings.BaseUrl) ||
            string.IsNullOrWhiteSpace(_settings.TokenEndpoint) ||
            string.IsNullOrWhiteSpace(_settings.ClientId) ||
            string.IsNullOrWhiteSpace(_settings.ClientSecret))
        {
            throw new InvalidOperationException("ANB settings are not properly configured.");
        }

        var request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(_settings.BaseUrl), _settings.TokenEndpoint));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", _settings.ClientId),
            new KeyValuePair<string, string>("client_secret", _settings.ClientSecret)
        });

        request.Content = content;

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Token request failed: {response.StatusCode} - {errorBody}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<AnbTokenResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return tokenResponse?.AccessToken;
    }

    private class AnbTokenResponse
    {
        public string? AccessToken { get; set; }
        public int? ExpiresIn { get; set; }
        public string? TokenType { get; set; }
    }

    public async Task<string> GetAccountBalanceAsync(string accountNumber)
    {
        try
        {
            var token = await GetAccessTokenAsync();
            var baseUrl = _configuration["AnbConnect:BaseUrl"]
                ?? throw new InvalidOperationException("ANB base URL is missing in configuration.");

            var url = $"{baseUrl}/accounts/{accountNumber}/balance";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            // Network or protocol errors
            throw new ApplicationException($"HTTP error while fetching balance for account {accountNumber}: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            // Timeout
            throw new ApplicationException($"Request timed out while fetching balance for account {accountNumber}.", ex);
        }
        catch (Exception ex)
        {
            // General errors
            throw new ApplicationException($"Unexpected error while fetching balance for account {accountNumber}: {ex.Message}", ex);
        }
    }

    public async Task<bool> VerifyAccountAsync(string accountNumber)
    {
        var requestUrl = $"{_settings.BaseUrl}/apis/api/account-verification-service";

        var requestBody = new
        {
            accountNumber = accountNumber
        };

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl)
        {
            Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json")
        };

        var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}"));
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", authToken);

        var response = await _httpClient.SendAsync(requestMessage);
        if (!response.IsSuccessStatusCode) return false;

        var responseContent = await response.Content.ReadAsStringAsync();
        return responseContent.Contains("Valid", StringComparison.OrdinalIgnoreCase);
    }
}
