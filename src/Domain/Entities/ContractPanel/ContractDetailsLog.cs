using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Entities.ContractPanel;
public class ContractDetailsLog : BaseAuditableEntity
{
    public int ContractId { get; set; }
    public string? Operation { get; set; }  // e.g. CREATE, UPDATE, DELETE, etc.
    public string? ChangedFields { get; set; }  // comma-separated changed fields
    public string? PreviousData { get; set; }   // JSON (string)
    public string? NewData { get; set; }        // JSON (string)
    public string? Remarks { get; set; }
    public string? ChangedBy { get; set; }
    public string? Source { get; set; }         // Optional: API/Admin/etc.
}
