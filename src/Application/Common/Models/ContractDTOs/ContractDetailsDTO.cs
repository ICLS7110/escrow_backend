using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Models.ContractDTOs
{
    public class ContractDetailsDTO
    {
        public int Id { get; set; }
        public string Role { get; set; } = string.Empty;
        public string ContractTitle { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public string ServiceDescription { get; set; } = string.Empty;
        public string? AdditionalNote { get; set; }
        public string FeesPaidBy { get; set; } = string.Empty;
        public decimal? FeeAmount { get; set; }
        public string? BuyerName { get; set; }
        public string? BuyerMobile { get; set; }
        public string? SellerName { get; set; }
        public string? SellerMobile { get; set; }
        public string Status { get; set; } = string.Empty;

        public List<MileStoneDTO> MileStones { get; set; } = new List<MileStoneDTO>();
    }
}
