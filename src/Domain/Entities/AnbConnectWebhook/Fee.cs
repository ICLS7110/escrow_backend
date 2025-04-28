using System.Text.Json.Serialization;

namespace Escrow.Api.Domain.Entities.AnbConnectWebhook;
public class Fee
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("status")]
    public required string Status { get; set; }

    [JsonPropertyName("segment")]
    public required string Segment { get; set; }

    [JsonPropertyName("category")]
    public required string Category { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("feeAmount")]
    public required string FeeAmount { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTimeOffset UpdatedAt { get; set; }

    [JsonPropertyName("vatAmount")]
    public required string VatAmount { get; set; }

    [JsonPropertyName("feeIncluded")]
    public bool FeeIncluded { get; set; }

    [JsonPropertyName("totalAmount")]
    public required string TotalAmount { get; set; }

    [JsonPropertyName("waivedAmount")]
    public required string WaivedAmount { get; set; }

    [JsonPropertyName("vatPercentage")]
    public required string VatPercentage { get; set; }

    [JsonPropertyName("feesAndVATAccount")]
    public required string FeesAndVATAccount { get; set; }
}
