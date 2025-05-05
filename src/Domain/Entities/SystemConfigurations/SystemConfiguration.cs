using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Entities.SystemConfigurations;
public class SystemConfiguration : BaseAuditableEntity
{
    public string? Key { get; set; } // e.g., "MonthlySpendingLimit"
    public string? Value { get; set; } // stored as string, parse as needed
}
