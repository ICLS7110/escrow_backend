using System;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Entities.ContractPanel;
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

        private string GenerateInvitationLink(string sellerPhone, string buyerPhone)
        {
            return $"https://yourdomain.com/invite?seller={sellerPhone}&buyer={buyerPhone}";
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

            // Create an invitation record (storing phone numbers directly)
            var invitation = new SellerBuyerInvitation
            {
                SellerPhoneNumber = request.SellerMobileNumber,
                BuyerPhoneNumber = request.BuyerMobileNumber,
                InvitationLink = GenerateInvitationLink(request.SellerMobileNumber, request.BuyerMobileNumber),
                Status = "Pending",
                ContractId = request.ContractId
            };

            _context.SellerBuyerInvitations.Add(invitation);
            await _context.SaveChangesAsync(cancellationToken);

            return invitation.InvitationLink; // Return the generated invitation link
        }
    }
}
