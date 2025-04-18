using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Domain.Entities.ContractPanel;

namespace Escrow.Api.Application.Common.Models.ContractDTOs
{
    public class MileStoneDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset DueDate { get; set; }
        public string? Documents { get; set; }
        public string? Status { get; set; }
        public int? ContractId { get; set; }
        public string? MileStoneEscrowAmount { get; set; }
        public string? MileStoneTaxAmount { get; set; }
    }

   
}
