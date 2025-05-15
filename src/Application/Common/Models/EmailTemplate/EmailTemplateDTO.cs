using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Models.EmailTemplate;
public class EmailTemplateDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // e.g., "Welcome Email"
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true; // Indicates if the template is currently active
    public bool IsDeleted { get; set; } = false; // Soft delete flag
}
