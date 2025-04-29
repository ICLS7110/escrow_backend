using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Escrow.Api.Domain.Entities.AdminPanel
{
    public class AddSubAdminCommand : BaseAuditableEntity
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? Password { get; set; }
    }
}
