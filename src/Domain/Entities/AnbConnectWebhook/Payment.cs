using System.Text.Json.Serialization;

namespace Escrow.Api.Domain.Entities.AnbConnectWebhook;
public class Payment
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("fee")]
    public required Fee Fee { get; set; }

    [JsonPropertyName("amount")]
    public required string Amount { get; set; }

    [JsonPropertyName("status")]
    public required string Status { get; set; }

    [JsonPropertyName("channel")]
    public required string Channel { get; set; }

    [JsonPropertyName("currency")]
    public required string Currency { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("narrative")]
    public required string Narrative { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTimeOffset UpdatedAt { get; set; }

    [JsonPropertyName("valueDate")]
    public required string ValueDate { get; set; }

    [JsonPropertyName("uetrNumber")]
    public required string UetrNumber { get; set; }

    [JsonPropertyName("requesterId")]
    public required string RequesterId { get; set; }

    [JsonPropertyName("debitAccount")]
    public required string DebitAccount { get; set; }

    [JsonPropertyName("exchangeRate")]
    public required string ExchangeRate { get; set; }

    [JsonPropertyName("creditAccount")]
    public required string CreditAccount { get; set; }

    [JsonPropertyName("debitedAmount")]
    public required string DebitedAmount { get; set; }

    [JsonPropertyName("orderingParty")]
    public required string OrderingParty { get; set; }

    [JsonPropertyName("sequenceNumber")]
    public required string SequenceNumber { get; set; }

    [JsonPropertyName("beneficiaryName")]
    public required string BeneficiaryName { get; set; }

    [JsonPropertyName("reasonOfFailure")]
    public required string ReasonOfFailure { get; set; }

    [JsonPropertyName("purposeOfTransfer")]
    public required string PurposeOfTransfer { get; set; }

    [JsonPropertyName("destinationBankBIC")]
    public required string DestinationBankBIC { get; set; }

    [JsonPropertyName("transactionComment")]
    public required string TransactionComment { get; set; }

    [JsonPropertyName("beneficiaryAddress1")]
    public required string BeneficiaryAddress1 { get; set; }

    [JsonPropertyName("beneficiaryAddress2")]
    public required string BeneficiaryAddress2 { get; set; }

    [JsonPropertyName("intermediateAccount")]
    public required string IntermediateAccount { get; set; }

    [JsonPropertyName("orderingPartyAddress1")]
    public required string OrderingPartyAddress1 { get; set; }

    [JsonPropertyName("orderingPartyAddress2")]
    public required string OrderingPartyAddress2 { get; set; }

    [JsonPropertyName("orderingPartyAddress3")]
    public required string OrderingPartyAddress3 { get; set; }

    [JsonPropertyName("externalReferenceNumber")]
    public required string ExternalReferenceNumber { get; set; }

    [JsonPropertyName("transactionReferenceNumber")]
    public required string TransactionReferenceNumber { get; set; }

    // Properties seen in payment.completed example but not payment.received
    [JsonPropertyName("iban")]
    public required Iban Iban { get; set; } // Add Iban class if needed
}
