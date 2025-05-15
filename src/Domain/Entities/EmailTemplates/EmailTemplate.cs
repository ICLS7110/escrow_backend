using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Entities.EmailTemplates;
public class EmailTemplate : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty; // Template name (e.g., Welcome Email, Forgot Password)
    public string Subject { get; set; } = string.Empty; // Email subject
    public string Body { get; set; } = string.Empty; // Email content (HTML or plain text)
    public bool IsActive { get; set; } = true; // Indicates if the template is active
    public bool IsDeleted { get; set; } = false; // Soft delete flag
}
