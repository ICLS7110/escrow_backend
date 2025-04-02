using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Entities.Pages;
public class Page : BaseAuditableEntity
{
    public string Title { get; set; } = string.Empty; // e.g., "Terms and Conditions"
    public string Slug { get; set; } = string.Empty;  // e.g., "terms-and-conditions"
    public string Content { get; set; } = string.Empty; // Policy text
    public bool? IsActive { get; set; }
    public bool? IsDeleted { get; set; }
}
