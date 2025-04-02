using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Models;
public class TransactionDTO
{
    public int Id { get; set; }
    public DateTime TransactionDateTime { get; set; }
    public decimal TransactionAmount { get; set; }
    public string TransactionType { get; set; } = string.Empty; // In Escrow, Released, Withdrawn
    public string FromPayee { get; set; } = string.Empty; // Buyer or Seller initiating the transaction
    public string ToRecipient { get; set; } = string.Empty; // Buyer, Seller, or third-party recipient
    public int? ContractId { get; set; } // Linked contract ID for traceability
}
