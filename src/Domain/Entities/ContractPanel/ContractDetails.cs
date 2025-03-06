using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Domain.Entities.UserPanel;

namespace Escrow.Api.Domain.Entities.ContractPanel;
public class ContractDetails : BaseAuditableEntity
{
    public string Role { get; set; } = string.Empty;
    public string ContractTitle { get; set; } = string.Empty;
    public string ServiceType { get; set; }= string.Empty;
    public string ServiceDescription { get; set; } = string.Empty;  
    public string? AdditionalNote {  get; set; }
    public string FeesPaidBy { get; set; } = string.Empty;
    public decimal? FeeAmount { get; set; }
    public string? BuyerName {  get; set; }
    public string? BuyerMobile { get; set; }
    public string? SellerName {  get; set; }
    public string? SellerMobile { get;  set; }
    public string Status {  get; set; } = string.Empty;
    public string? StatusReason { get; set; }    
    public int? BuyerDetailsId { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? EscrowTax { get; set; }
    public UserDetail? BuyerDetails { get; set; }
    public int? SellerDetailsId { get; set; }
    public UserDetail? SellerDetails { get; set; }

    public int? UserDetailId { get; set; }
    public UserDetail? UserDetail { get; set; }
    
    public ICollection<MileStone>? MileStones { get; set; } 
}
