using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Domain.Common
{
    public abstract class BaseAuditableEntity : BaseEntity
    {
        public DateTimeOffset CreatedDate { get; set; }

        public string? CreatedBy { get; set; }

        public DateTimeOffset LastModifiedDate { get; set; }

        public string? LastModifiedBy { get; set; }
    }
}
