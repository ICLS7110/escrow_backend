using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Domain.Entities.UserPanel;

namespace Escrow.Api.Domain.Entities.ContractPanel
{
    public class SellerBuyerInvitation: BaseAuditableEntity
    {
        public int SellerId { get; set; }
        public int BuyerId { get; set; }
        public string? BuyerPhoneNumber { get; set; }
        public string? SellerPhoneNumber { get; set; }
        public string? InvitationLink { get; set; }
        public string Status { get; set; } = "Pending";
        public int ContractId { get; set; }
        //public UserDetail? Seller { get; set; }
        //public UserDetail? Buyer { get; set; }
    }
}
