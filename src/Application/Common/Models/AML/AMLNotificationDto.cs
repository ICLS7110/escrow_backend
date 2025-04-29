using System;

namespace Escrow.Api.Application.Common.Models.AML
{
    public class AMLNotificationDto
    {
        public int Id { get; set; }  
        public string TransactionId { get; set; } = "N/A";  
        public string UserId { get; set; } = "Unknown";  
        public string Message { get; set; } = "No message available";  
        public bool IsRead { get; set; }  
        public DateTime CreatedAt { get; set; }  
    }
}
