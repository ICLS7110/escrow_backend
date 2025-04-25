using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Models;
public class NotificationDTO
{
    public int Id { get; set; }
    public int FromID { get; set; }
    public int ToID { get; set; }
    public int ContractId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool? IsRead { get; set; }
    public string? unreadCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NotificationReadStatusResultDto
{
    public int NotificationId { get; set; }
    public bool IsRead { get; set; }
}

