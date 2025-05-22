using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models.Payments;
using Escrow.Api.Application.Payments.Commands;
using Microsoft.Extensions.Configuration;

namespace Escrow.Api.Application.Common.Helpers
{
    public class MyFatoorahService : IMyFatoorahService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;
        private readonly string _token;
        private readonly string _callbackUrl;
        private readonly string _errorUrl;
        private readonly IJwtService _jwtService;
        private readonly IApplicationDbContext _context;


        public MyFatoorahService(HttpClient httpClient, IConfiguration configuration, IJwtService jwtService, IApplicationDbContext context)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _baseUrl = _configuration["MyFatoorah:BaseUrl"] ?? throw new ArgumentNullException("MyFatoorah:BaseUrl");
            _token = _configuration["MyFatoorah:Token"] ?? throw new ArgumentNullException("MyFatoorah:Token");
            _callbackUrl = _configuration["MyFatoorah:CallBackUrl"] ?? "https://welink-sa.com/";
            _errorUrl = _configuration["MyFatoorah:ErrorUrl"] ?? "https://welink-sa.com/";
            _jwtService = jwtService;
            _context = context;
        }

        public async Task<List<PaymentMethodDto>> InitiatePaymentAsync(decimal invoiceAmount, string currencyIso)
        {
            try
            {
                var requestBody = new
                {
                    InvoiceAmount = invoiceAmount,
                    CurrencyIso = currencyIso
                };

                var requestUri = new Uri(new Uri(_baseUrl), "v2/InitiatePayment");

                var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
                {
                    Content = JsonContent.Create(requestBody)
                };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                var response = await _httpClient.SendAsync(request);

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<MyFatoorahInitiateResponse>();

                return result?.Data?.PaymentMethods ?? new List<PaymentMethodDto>();

            }
            catch (HttpRequestException httpEx)
            {
                throw new Exception($"Error calling MyFatoorah API: {httpEx.Message}", httpEx);
            }
            catch (NotSupportedException notSupEx)
            {
                throw new Exception($"The content type is not supported: {notSupEx.Message}", notSupEx);
            }
            catch (JsonException jsonEx)
            {
                throw new Exception($"Error parsing response from MyFatoorah API: {jsonEx.Message}", jsonEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error in InitiatePaymentAsync: {ex.Message}", ex);
            }
        }

        public async Task<ExecutePaymentResultDto> ExecutePaymentAsync(ExecutePaymentCommand command)
        {
            var userId = _jwtService.GetUserId().ToInt();
            var user = _context.UserDetails
                .AsNoTracking()
                .FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            try
            {
                var body = new
                {
                    PaymentMethodId = command.PaymentMethodId,
                    InvoiceValue = command.InvoiceAmount,
                    CurrencyIso = command.CurrencyIso,
                    CustomerName = user.FullName ?? "N/A",
                    CustomerEmail = user.EmailAddress ?? "noemail@example.com",
                    CustomerMobile = PhoneNumberHelper.ExtractPhoneNumberWithoutCountryCode(user.PhoneNumber) ?? "0000000000",
                    CallBackUrl = _callbackUrl,
                    ErrorUrl = _errorUrl,
                    DisplayCurrencyIso = "SAR",
                    Language = "en",
                };

                var requestUri = new Uri(new Uri(_baseUrl), "v2/ExecutePayment");

                var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
                {
                    Content = JsonContent.Create(body)
                };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                var response = await _httpClient.SendAsync(request);

                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"MyFatoorah API Error (StatusCode: {response.StatusCode}): {jsonResponse}");
                }

                using var document = JsonDocument.Parse(jsonResponse);
                var root = document.RootElement;

                var dataElement = root.GetProperty("Data");

                var dto = JsonSerializer.Deserialize<ExecutePaymentResultDto>(
                    dataElement.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                 
                return dto!;
            }
            catch (Exception ex)
            {
                throw new Exception($"ExecutePaymentAsync failed: {ex.Message}", ex);
            }
        }


        



        // Local response models
        private class MyFatoorahInitiateResponse
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; } = string.Empty;
            public List<ValidationError> ValidationErrors { get; set; } = new();
            public PaymentMethodsData Data { get; set; } = new();
        }

        private class ValidationError
        {
            public string Name { get; set; } = string.Empty;
            public string Error { get; set; } = string.Empty;
        }

        private class PaymentMethodsData
        {
            public List<PaymentMethodDto> PaymentMethods { get; set; } = new();
        }
    }
}







//public async Task<PaymentMethodDto> GetPaymentMethodByIdAsync(int paymentMethodId)
//{
//    try
//    {
//        var requestUri = new Uri(new Uri(_baseUrl), $"v2/GetPaymentMethod/{paymentMethodId}");
//        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
//        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
//        var response = await _httpClient.SendAsync(request);
//        response.EnsureSuccessStatusCode();
//        var result = await response.Content.ReadFromJsonAsync<PaymentMethodDto>();
//        return result!;
//    }
//    catch (Exception ex)
//    {
//        throw new Exception($"GetPaymentMethodByIdAsync failed: {ex.Message}", ex);
//    }
//}

//public async Task<MyFatoorahPaymentStatusResponse> GetPaymentStatusAsync(string paymentId)
//{
//    var requestBody = new
//    {
//        Key = paymentId,
//        KeyType = "PaymentId"
//    };

//    var requestUri = new Uri(new Uri(_baseUrl), "v2/GetPaymentStatus");

//    var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
//    {
//        Content = JsonContent.Create(requestBody)
//    };

//    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

//    var response = await _httpClient.SendAsync(request);
//    response.EnsureSuccessStatusCode();

//    var paymentStatus = await response.Content.ReadFromJsonAsync<MyFatoorahPaymentStatusResponse>();

//    if (paymentStatus == null)
//    {
//        throw new Exception("Failed to deserialize payment status response from MyFatoorah.");
//    }

//    return paymentStatus;

//}
