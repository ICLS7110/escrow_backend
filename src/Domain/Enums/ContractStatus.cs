using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))] // ✅ Converts Enum to String in API
    public enum ContractStatus
    {
        Pending,
        Accepted,
        Rejected,
        Expired
    }

}
