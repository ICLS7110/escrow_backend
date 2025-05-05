using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Entities.Commissions;
public class CommissionMaster : BaseAuditableEntity
{
    public decimal CommissionRate { get; set; }
    public bool AppliedGlobally { get; set; }
    public string? MinAmount { get; set; }
    public string? TransactionType { get; set; } // Service, Product, etc.
    public decimal TaxRate { get; set; } // Tax rate per transaction type
}
