using System.Text.Json.Serialization;

namespace Escrow.Api.Domain.Entities.AnbConnectWebhook;

public class PaymentCompletedPayload : NotificationPayload
{
    [JsonPropertyName("payment")]
    public required Payment Payment { get; set; }
}
