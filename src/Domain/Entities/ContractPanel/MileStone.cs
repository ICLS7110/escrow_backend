using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Entities.ContractPanel;
public class MileStone : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset DueDate { get; set; }
    public string? Documents {  get; set; }
    
    public int? ContractId { get; set; }
    [ForeignKey(nameof(ContractId))]
    public ContractDetails? ContractDetails { get; set; }
}
