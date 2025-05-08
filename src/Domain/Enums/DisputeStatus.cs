using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))] // Converts Enum to String in API
public enum DisputeStatus
{
    Pending,
    Resolved,
    DisputeWin,
    InReview,
    WaitingForResponse,
    DisputeLoss,
    WaitingForBuyer,
    WaitingForSeller
}
