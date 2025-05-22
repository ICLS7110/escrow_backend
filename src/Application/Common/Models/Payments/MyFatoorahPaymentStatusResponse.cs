using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Models.Payments;
public class MyFatoorahPaymentStatusResponse
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public PaymentStatusData? Data { get; set; }
}

//public class PaymentStatusData
//{
//    public long InvoiceId { get; set; }
//    public string? InvoiceStatus { get; set; } // "Paid", "Pending", "Failed", etc.
//    public decimal InvoiceValue { get; set; }
//    public string? CustomerName { get; set; }
//    public string? CustomerMobile { get; set; }
//    public string? CustomerEmail { get; set; }
//    public string? CreatedDate { get; set; }
//    public string? ExpiryDate { get; set; }
//    public string? InvoiceDisplayValue { get; set; }
//    public string? Currency { get; set; }
//    public List<InvoiceTransaction>? InvoiceTransactions { get; set; }
//}

//public class InvoiceTransaction
//{
//    public string? PaymentGateway { get; set; }         // e.g., "ApplePay"
//    public string? TransactionStatus { get; set; }       // "Success", "Failed"
//    public string? PaymentId { get; set; }
//    public string? AuthorizationId { get; set; }
//    public string? ReferenceId { get; set; }
//    public string? TransactionValue { get; set; }
//    public string? PaidCurrency { get; set; }
//    public string? PaidCurrencyValue { get; set; }
//    public string? TransactionDate { get; set; }
//    public string? Error { get; set; }
//    public string? GatewayReference { get; set; }
//}

