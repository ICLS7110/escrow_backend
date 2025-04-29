namespace Escrow.Api.Domain.Entities.AnbConnectWebhook;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("AnbWebhookLogs")]
public class AnbWebhookLog : BaseAuditableEntity
{
    [Required]
    public DateTimeOffset ReceivedAt { get; set; }
    [Required]
    public required string EventType { get; set; }
    [Required]
    public required string Payload { get; set; }
    public string? ErrorMessage { get; set; }
}
