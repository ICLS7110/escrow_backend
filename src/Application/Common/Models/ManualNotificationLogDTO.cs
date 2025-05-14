using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Models;
public class ManualNotificationLogDTO
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Message { get; set; } = null!;
    public bool SentToAll { get; set; }
    public List<int>? SentToUserIds { get; set; }
    public DateTimeOffset Created { get; set; }
}

