using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Entities.Notifications;
public class ManualNotificationLog : BaseAuditableEntity
{
    public string Title { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Message { get; set; } = null!;
    public bool SentToAll { get; set; }
    // Stores selected user IDs in JSON format
    public string? SentTo { get; set; }
}
