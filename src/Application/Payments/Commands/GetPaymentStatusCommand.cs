using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Models.Payments;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Escrow.Api.Application.Payments.Commands;
public class GetPaymentStatusCommand : IRequest<Result<PaymentStatusData>>
{
    public string Key { get; set; } = string.Empty;   // Invoice key or ID from your app
    public string KeyType { get; set; } = "InvoiceId";  // Usually "InvoiceId" or "PaymentId", per MyFatoorah docs

    public GetPaymentStatusCommand(string key, string keyType = "InvoiceId")
    {
        Key = key;
        KeyType = keyType;
    }
}


public class GetPaymentStatusCommandHandler : IRequestHandler<GetPaymentStatusCommand, Result<PaymentStatusData>>
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _token;

    public GetPaymentStatusCommandHandler(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _baseUrl = config["MyFatoorah:BaseUrl"]
            ?? throw new ArgumentNullException(nameof(config), "MyFatoorah:BaseUrl is missing from configuration");
        _token = config["MyFatoorah:Token"]
            ?? throw new ArgumentNullException(nameof(config), "MyFatoorah:Token is missing from configuration");
    }


    public async Task<Result<PaymentStatusData>> Handle(GetPaymentStatusCommand request, CancellationToken cancellationToken)
    {
        var requestUri = new Uri(new Uri(_baseUrl), "v2/GetPaymentStatus");

        var body = new
        {
            Key = request.Key,
            KeyType = request.KeyType
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(body)
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            return Result<PaymentStatusData>.Failure((int)response.StatusCode, $"API Error: {errorContent}");
        }

        var responseData = await response.Content.ReadFromJsonAsync<PaymentStatusResponse>(cancellationToken: cancellationToken);

        if (responseData == null || !responseData.IsSuccess || responseData.Data == null)
        {
            var errorMsg = responseData?.Message ?? "Failed to retrieve payment status";
            return Result<PaymentStatusData>.Failure(StatusCodes.Status400BadRequest, errorMsg);
        }

        return Result<PaymentStatusData>.Success(StatusCodes.Status200OK, "Payment status retrieved successfully", responseData.Data);
    }
}
