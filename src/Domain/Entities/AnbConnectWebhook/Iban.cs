using System.Text.Json.Serialization;

namespace Escrow.Api.Domain.Entities.AnbConnectWebhook;

public class Iban
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("bic")]
    public required string Bic { get; set; }
    [JsonPropertyName("city")]
    public required string City { get; set; }
}
