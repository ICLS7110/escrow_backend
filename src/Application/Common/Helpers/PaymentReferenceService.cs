using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Helpers;
public class PaymentReferenceService
{
    public string GenerateReference(char indicator, int customerId, int contractId, int? milestoneId = null)
    {
        if (indicator != 'S' && indicator != 'B')
            throw new ArgumentException("Indicator must be 'S' or 'B'");

        string customerStr = customerId.ToString("D3");       // 3 digits
        string contractStr = contractId.ToString("D6");       // 6 digits
        string milestoneStr = milestoneId.HasValue ? milestoneId.Value.ToString("D2") : "";

        return $"{indicator}{customerStr}{contractStr}{milestoneStr}";
    }
}

