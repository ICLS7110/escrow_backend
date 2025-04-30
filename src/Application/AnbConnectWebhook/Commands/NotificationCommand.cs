namespace Escrow.Api.Application.AnbConnectWebhook.Commands;
using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;

public class WebhookNotificationCommand : IRequest
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("payload")]
    public JsonElement Payload { get; set; }
}
