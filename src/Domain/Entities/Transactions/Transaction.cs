using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Entities.Transactions;
public class Transaction : BaseAuditableEntity
{
    public DateTime TransactionDateTime { get; set; }
    public decimal TransactionAmount { get; set; }
    public string? TransactionType { get; set; } // In Escrow, Released, Withdrawn
    public string? FromPayee { get; set; } // Buyer or Seller initiating the transaction
    public string? ToRecipient { get; set; } // Buyer, Seller, or third-party recipient
    public int? ContractId { get; set; } // Linked contract ID for traceability
}
