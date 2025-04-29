using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Enums;
public enum AMLTransactionStatus
{
    Pending,
    Approved,
    Held,
    FlaggedForInvestigation
}
