using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Models.Payments;
public class PaymentStatusResponse
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public List<ValidationError>? ValidationErrors { get; set; }
    public PaymentStatusData? Data { get; set; }
}

public class ValidationError
{
    public string? Name { get; set; }
    public string? Error { get; set; }
}

public class PaymentStatusData
{
    public long InvoiceId { get; set; }
    public string? InvoiceStatus { get; set; }
    public string? InvoiceReference { get; set; }
    public string? CustomerReference { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ExpiryDate { get; set; }
    public string? ExpiryTime { get; set; }
    public decimal InvoiceValue { get; set; }
    public string? Comments { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerMobile { get; set; }
    public string? CustomerEmail { get; set; }
    public string? UserDefinedField { get; set; }
    public string? InvoiceDisplayValue { get; set; }
    public decimal DueDeposit { get; set; }
    public string? DepositStatus { get; set; }
    public List<InvoiceItem>? InvoiceItems { get; set; }
    public List<InvoiceTransaction>? InvoiceTransactions { get; set; }
    public List<Supplier>? Suppliers { get; set; }
}

public class InvoiceItem
{
    public string? ItemName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Weight { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public decimal Depth { get; set; }
}

public class InvoiceTransaction
{
    public DateTime TransactionDate { get; set; }
    public string? PaymentGateway { get; set; }
    public string? ReferenceId { get; set; }
    public string? TrackId { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentId { get; set; }
    public string? AuthorizationId { get; set; }
    public string? TransactionStatus { get; set; }
    public string? TransationValue { get; set; }
    public string? CustomerServiceCharge { get; set; }
    public string? TotalServiceCharge { get; set; }
    public string? DueValue { get; set; }
    public string? PaidCurrency { get; set; }
    public string? PaidCurrencyValue { get; set; }
    public string? VatAmount { get; set; }
    public string? IpAddress { get; set; }
    public string? Country { get; set; }
    public string? Currency { get; set; }
    public string? Error { get; set; }
    public string? CardNumber { get; set; }
    public string? ErrorCode { get; set; }
    public string? ECI { get; set; }
    public object? Card { get; set; } // Can be expanded later if needed
}

public class Supplier
{
    public int SupplierCode { get; set; }
    public string? SupplierName { get; set; }
    public decimal InvoiceShare { get; set; }
    public decimal ProposedShare { get; set; }
    public decimal DepositShare { get; set; }
}


public class PaymentResponse
{
    public string? ResponseCode { get; set; }
    public string? ResponseMessage { get; set; }
    public string? TransactionReference { get; set; }
}


public class SanctionsCheckResponse
{
    public bool IsSanctioned { get; set; }
    public string? Message { get; set; }
}

