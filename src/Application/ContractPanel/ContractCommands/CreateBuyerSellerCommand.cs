using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Entities.ContractPanel;
using Escrow.Api.Domain.Entities.UserPanel;

namespace Escrow.Api.Application.ContractPanel.ContractCommands
{

    public class CreateBuyerSellerCommand : IRequest<string>
    {
        public string? SellerMobileNumber { get; set; }
        public string? BuyerMobileNumber { get; set; }
        public int ContractId { get; set; }  // New property for ContractId
    }

    public class CreateBuyerSellerHandler : IRequestHandler<CreateBuyerSellerCommand, string>
    {
        private readonly IApplicationDbContext _context;

        public CreateBuyerSellerHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        private string GenerateInvitationLink(int sellerId, int buyerId)
        {
            return $"https://yourdomain.com/invite?seller={sellerId}&buyer={buyerId}";
        }

        public async Task<string> Handle(CreateBuyerSellerCommand request, CancellationToken cancellationToken)
        {
            // Find the Seller ID using Phone Number
            var seller = await _context.UserDetails
                .Where(x => x.PhoneNumber == request.SellerMobileNumber)
                .Select(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (seller == 0)
            {
                throw new ValidationException("Seller with the given phone number does not exist.");
            }

            // Find the Buyer ID using Phone Number
            var buyer = await _context.UserDetails
                .Where(x => x.PhoneNumber == request.BuyerMobileNumber)
                .Select(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (buyer == 0)
            {
                throw new ValidationException("Buyer with the given phone number does not exist.");
            }

            // Create an invitation record
            var invitation = new SellerBuyerInvitation
            {
                SellerId = seller,  // Retrieved Seller ID
                BuyerId = buyer,    // Retrieved Buyer ID
                InvitationLink = GenerateInvitationLink(seller, buyer),
                Status = "Pending",  // Invitation status
                ContractId = request.ContractId  // Save the ContractId
            };

            _context.SellerBuyerInvitations.Add(invitation);
            await _context.SaveChangesAsync(cancellationToken);

            return invitation.InvitationLink;  // Return the generated invitation link
        }
    }
    //public class CreateBuyerSellerCommand : IRequest<string>
    //{
    //    public int UserId { get; set; }
    //    public string? BuyerMobileNumber { get; set; }
    //}

    //public class CreateBuyerSellerHandler : IRequestHandler<CreateBuyerSellerCommand, string>
    //{
    //    private readonly IApplicationDbContext _context;

    //    public CreateBuyerSellerHandler(IApplicationDbContext context)
    //    {
    //        _context = context;
    //    }
    //    private string GenerateInvitationLink(int sellerId, int buyerId)
    //    {
    //        return $"https://yourdomain.com/invite?seller={sellerId}&buyer={buyerId}";
    //    }

    //    public async Task<string> Handle(CreateBuyerSellerCommand request, CancellationToken cancellationToken)
    //    {
    //        // Find the Buyer ID using Phone Number
    //        var buyer = await _context.UserDetails
    //            .Where(x => x.PhoneNumber == request.BuyerMobileNumber)
    //            .Select(x => x.Id) // Select only the ID for efficiency
    //            .FirstOrDefaultAsync(cancellationToken);

    //        if (buyer == 0) // If Buyer not found
    //        {
    //            throw new ValidationException("Buyer with the given phone number does not exist.");
    //        }

    //        // Create an invitation record
    //        var invitation = new SellerBuyerInvitation
    //        {
    //            SellerId = request.UserId, // Seller ID from the request
    //            BuyerId = buyer, // Retrieved Buyer ID
    //            InvitationLink = GenerateInvitationLink(request.UserId, buyer), // Function to generate link
    //            Status = "Pending"
    //        };

    //        _context.SellerBuyerInvitations.Add(invitation);
    //        await _context.SaveChangesAsync(cancellationToken);

    //        return invitation.InvitationLink;
    //    }

    //}
}
