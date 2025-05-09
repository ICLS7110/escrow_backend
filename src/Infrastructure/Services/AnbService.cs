using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Escrow.Api.Infrastructure.Services;
public class AnbService : IAnbService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AnbService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    private async Task<string> GetAccessTokenAsync()
    {
        try
        {
            var tokenUrl = _configuration["AnbConnect:TokenUrl"]
                ?? throw new InvalidOperationException("ANB token URL is missing.");

            var clientId = _configuration["AnbConnect:ClientId"]
                ?? throw new InvalidOperationException("ANB Client ID is missing.");

            var clientSecret = _configuration["AnbConnect:ClientSecret"]
                ?? throw new InvalidOperationException("ANB Client Secret is missing.");

            var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
            {
                Content = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret)
            })
            };

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new ApplicationException($"Failed to retrieve access token. Status: {response.StatusCode}, Body: {json}");
            }

            Console.WriteLine("Access Token Response: " + json);

            var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("access_token").GetString()
                ?? throw new InvalidOperationException("Access token not returned.");
        }
        catch (HttpRequestException ex)
        {
            throw new ApplicationException("HTTP request failed while fetching access token.", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ApplicationException("Timeout occurred while fetching access token.", ex);
        }
        catch (JsonException ex)
        {
            throw new ApplicationException("Invalid JSON returned while parsing access token.", ex);
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Unexpected error occurred while retrieving access token.", ex);
        }
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

}
