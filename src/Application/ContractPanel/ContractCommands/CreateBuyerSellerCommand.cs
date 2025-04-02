using System;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Entities;
using Escrow.Api.Domain.Entities.ContractPanel;
using Escrow.Api.Domain.Entities.UserPanel;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Escrow.Api.Application.ContractPanel.ContractCommands
{
    public class CreateBuyerSellerCommand : IRequest<string>
    {
        public string SellerMobileNumber { get; set; } = string.Empty;
        public string BuyerMobileNumber { get; set; } = string.Empty;
        public int ContractId { get; set; }
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
            return $"https://yourdomain.com/invite?sellerId={sellerId}&buyerId={buyerId}";
        }

        public async Task<string> Handle(CreateBuyerSellerCommand request, CancellationToken cancellationToken)
        {
            // Check if the contract exists
            var contractExists = await _context.ContractDetails
                .AnyAsync(c => c.Id == request.ContractId, cancellationToken);

            if (!contractExists)
            {
                throw new ValidationException("The specified contract does not exist.");
            }

            // Find or create Seller
            var seller = await _context.UserDetails
                .FirstOrDefaultAsync(u => u.PhoneNumber == request.SellerMobileNumber, cancellationToken);

            if (seller == null)
            {
                seller = new UserDetail
                {
                    PhoneNumber = request.SellerMobileNumber,
                    Created = DateTime.UtcNow
                };
                _context.UserDetails.Add(seller);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Find or create Buyer
            var buyer = await _context.UserDetails
                .FirstOrDefaultAsync(u => u.PhoneNumber == request.BuyerMobileNumber, cancellationToken);

            if (buyer == null)
            {
                buyer = new UserDetail
                {
                    PhoneNumber = request.BuyerMobileNumber,
                    Created = DateTime.UtcNow
                };
                _context.UserDetails.Add(buyer);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Create an invitation record
            var invitation = new SellerBuyerInvitation
            {
                SellerId = seller.Id,
                BuyerId = buyer.Id,
                SellerPhoneNumber = request.SellerMobileNumber,
                BuyerPhoneNumber = request.BuyerMobileNumber,
                InvitationLink = GenerateInvitationLink(seller.Id, buyer.Id),
                Status = "Pending",
                ContractId = request.ContractId
            };

            _context.SellerBuyerInvitations.Add(invitation);
            // Save all changes
            await _context.SaveChangesAsync(cancellationToken);

            return invitation.InvitationLink; // Return the generated invitation link
        }
    }
}
