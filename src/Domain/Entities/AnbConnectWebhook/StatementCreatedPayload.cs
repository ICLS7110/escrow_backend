using System.Text.Json.Serialization;

namespace Escrow.Api.Domain.Entities.AnbConnectWebhook;
public class StatementCreatedPayload : NotificationPayload
{
    [JsonPropertyName("data")]
    public required string Data { get; set; }
}
