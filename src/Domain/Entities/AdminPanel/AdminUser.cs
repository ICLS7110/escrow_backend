using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Entities.AdminPanel
{
    public class AdminUser : BaseAuditableEntity
    {
        [Required]
        public string? Username { get; set; }
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? PasswordHash { get; set; }
        public string Role { get; set; } = "Admin"; // Default role
        public string? OTP { get; set; }

    }
}
