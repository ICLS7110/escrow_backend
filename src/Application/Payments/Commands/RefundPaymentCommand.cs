using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Models.Payments;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Escrow.Api.Application.Payments.Commands;
public class RefundPaymentCommand : IRequest<Result<RefundResponseData>>
{
    public string Key { get; set; } = string.Empty;      // InvoiceId, PaymentId, or RefundId etc.
    public string KeyType { get; set; } = "PaymentId";  // Usually "PaymentId" or "InvoiceId"
    public decimal RefundAmount { get; set; }           // Amount to refund
    public string? Reason { get; set; }                  // Optional refund reason

    public RefundPaymentCommand(string key, decimal refundAmount, string keyType = "PaymentId", string? reason = null)
    {
        Key = key;
        KeyType = keyType;
        RefundAmount = refundAmount;
        Reason = reason;
    }
}


public class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, Result<RefundResponseData>>
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _token;

    public RefundPaymentCommandHandler(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["MyFatoorah:BaseUrl"]
            ?? throw new ArgumentNullException(nameof(configuration), "MyFatoorah:BaseUrl config is missing");
        _token = configuration["MyFatoorah:Token"]
            ?? throw new ArgumentNullException(nameof(configuration), "MyFatoorah:Token config is missing");
    }

    public async Task<Result<RefundResponseData>> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        var url = new Uri(new Uri(_baseUrl), "v2/RefundPayment");

        var body = new
        {
            Key = request.Key,
            KeyType = request.KeyType,
            RefundAmount = request.RefundAmount,
            Reason = request.Reason
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(body)
        };

        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            return Result<RefundResponseData>.Failure((int)response.StatusCode, $"API Error: {errorContent}");
        }

        var responseData = await response.Content.ReadFromJsonAsync<RefundPaymentApiResponse>(cancellationToken: cancellationToken);

        if (responseData == null || !responseData.IsSuccess)
        {
            var msg = responseData?.Message ?? "Refund failed";
            return Result<RefundResponseData>.Failure(StatusCodes.Status400BadRequest, msg);
        }

        return Result<RefundResponseData>.Success(StatusCodes.Status200OK, "Refund processed successfully", responseData.Data);
    }

}

// Wrapper class for deserializing full response
public class RefundPaymentApiResponse
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public List<ValidationError>? ValidationErrors { get; set; }
    public RefundResponseData? Data { get; set; }
}
