using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Models.Payments;
public class ExecutePaymentResultDto
{
    public long InvoiceId { get; set; }
    public bool IsDirectPayment { get; set; }

    [JsonPropertyName("PaymentURL")]
    public string? PaymentUrl { get; set; }  // <-- make nullable

    public string? CustomerReference { get; set; }
    public string? UserDefinedField { get; set; }
    public string? RecurringId { get; set; }
}
