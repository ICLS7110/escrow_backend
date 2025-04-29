using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Domain.Entities.ContractPanel;

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
        public string? BuyerId { get; set; }
        public string? SellerId { get; set; }
        public string? SellerName { get; set; }
        public string? SellerMobile { get; set; }
        public string? CreatedBy { get; set; }
        public string? ContractDoc { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? EscrowTax { get; set; }
        public string? CountryCode { get; set; }
        public string? LastModifiedBy { get; set; }
        public string? BuyerPayableAmount { get; set; }
        public string? SellerPayableAmount { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset LastModified { get; set; }

        public List<MileStoneDTO> MileStones { get; set; } = new List<MileStoneDTO>();
        public SellerBuyerInvitation? InvitationDetails { get; set; }
    }

    public class ContractDTO
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
        public string? BuyerId { get; set; }
        public string? SellerId { get; set; }
        public string? SellerName { get; set; }
        public string? SellerMobile { get; set; }
        public string? SellerProfilePicture { get; set; }
        public string? BuyerProfilePicture { get; set; }
        public string? CreatedBy { get; set; }
        public string? ContractDoc { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? EscrowTax { get; set; }
        public string? CountryCode { get; set; }
        public string? LastModifiedBy { get; set; }
        public string? BuyerPayableAmount { get; set; }
        public string? SellerPayableAmount { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset LastModified { get; set; }

        public List<MileStoneDTO> MileStones { get; set; } = new List<MileStoneDTO>();
        public SellerBuyerInvitation? InvitationDetails { get; set; }
    }
}
