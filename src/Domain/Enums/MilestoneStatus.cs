using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Enums;
public enum MilestoneStatus
{
    Pending,
    InProgress,
    Completed,
    Cancelled,
    Disputed,
    Released,
    Refunded,
    Failed,
    OnHold,
    Escrow
}
