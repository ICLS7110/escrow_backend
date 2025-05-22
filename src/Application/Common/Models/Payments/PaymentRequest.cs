using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Models.Payments;
public class PaymentRequest
{
    public string? SequenceNumber { get; set; }       // Unique transaction ID
    public string? ValueDate { get; set; }            // Format: "yyMMdd"
    public string? Currency { get; set; }             // e.g., "SAR"
    public string? Amount { get; set; }               // e.g., "100.00"
    public string? DebitAccount { get; set; }         // Source account number
    public string? CreditAccount { get; set; }        // Destination account number
    public string? BeneficiaryName { get; set; }      // Beneficiary full name
    public string? PurposeOfTransfer { get; set; }    // e.g., "Personal Transfer"
}
