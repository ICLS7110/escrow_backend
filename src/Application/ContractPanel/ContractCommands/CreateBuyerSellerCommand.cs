
using System;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Constants;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Entities;
using Escrow.Api.Domain.Entities.ContractPanel;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateBuyerSellerHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GenerateInvitationLink(int sellerId, int buyerId)
        {
            return $"https://yourdomain.com/invite?sellerId={sellerId}&buyerId={buyerId}";
        }

        public async Task<string> Handle(CreateBuyerSellerCommand request, CancellationToken cancellationToken)
        {
            // Retrieve the current language from HttpContext
            var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

            // Check if the contract exists and retrieve it
            var contract = await _context.ContractDetails
                .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

            if (contract == null)
            {
                throw new ValidationException(AppMessages.Get("SpecifiedContract", language));
            }

            // Find or create Seller
            var seller = await _context.UserDetails
                .FirstOrDefaultAsync(u => u.PhoneNumber == request.SellerMobileNumber, cancellationToken);

            if (seller == null)
            {
                seller = new UserDetail
                {
                    UserId = Guid.NewGuid().ToString(),
                    PhoneNumber = request.SellerMobileNumber,
                    Created = DateTime.UtcNow,
                    Role = nameof(Roles.User)
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
                    UserId = Guid.NewGuid().ToString(),
                    PhoneNumber = request.BuyerMobileNumber,
                    Created = DateTime.UtcNow,
                    Role = nameof(Roles.User)
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
                Status = nameof(ContractStatus.Pending),
                ContractId = request.ContractId
            };

            _context.SellerBuyerInvitations.Add(invitation);

            // Update the contract status to "Pending"
            contract.Status = nameof(ContractStatus.Pending);
            _context.ContractDetails.Update(contract);

            // Save all changes
            await _context.SaveChangesAsync(cancellationToken);

            return invitation.InvitationLink;
        }
    }
}






































//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Constants;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Domain.Entities;
//using Escrow.Api.Domain.Entities.ContractPanel;
//using Escrow.Api.Domain.Entities.UserPanel;
//using Escrow.Api.Domain.Enums;
//using MediatR;
//using Microsoft.AspNetCore.Http;
//using Microsoft.EntityFrameworkCore;

//namespace Escrow.Api.Application.ContractPanel.ContractCommands
//{
//    public class CreateBuyerSellerCommand : IRequest<string>
//    {
//        public string SellerMobileNumber { get; set; } = string.Empty;
//        public string BuyerMobileNumber { get; set; } = string.Empty;
//        public int ContractId { get; set; }
//    }

//    public class CreateBuyerSellerHandler : IRequestHandler<CreateBuyerSellerCommand, string>
//    {
//        private readonly IApplicationDbContext _context;

//        public CreateBuyerSellerHandler(IApplicationDbContext context)
//        {
//            _context = context;
//        }

//        private string GenerateInvitationLink(int sellerId, int buyerId)
//        {
//            return $"https://yourdomain.com/invite?sellerId={sellerId}&buyerId={buyerId}";
//        }

//        public async Task<string> Handle(CreateBuyerSellerCommand request, CancellationToken cancellationToken)
//        {
//            // Check if the contract exists and retrieve it
//            var contract = await _context.ContractDetails
//                .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

//            if (contract == null)
//            {
//                throw new ValidationException(AppMessages.SpecifiedContract);
//            }

//            // Find or create Seller
//            var seller = await _context.UserDetails
//                .FirstOrDefaultAsync(u => u.PhoneNumber == request.SellerMobileNumber, cancellationToken);

//            if (seller == null)
//            {
//                seller = new UserDetail
//                {
//                    UserId = Guid.NewGuid().ToString(),
//                    PhoneNumber = request.SellerMobileNumber,
//                    Created = DateTime.UtcNow,
//                    Role = nameof(Roles.User)
//                };
//                _context.UserDetails.Add(seller);
//                await _context.SaveChangesAsync(cancellationToken);
//            }

//            // Find or create Buyer
//            var buyer = await _context.UserDetails
//                .FirstOrDefaultAsync(u => u.PhoneNumber == request.BuyerMobileNumber, cancellationToken);

//            if (buyer == null)
//            {
//                buyer = new UserDetail
//                {
//                    UserId = Guid.NewGuid().ToString(),
//                    PhoneNumber = request.BuyerMobileNumber,
//                    Created = DateTime.UtcNow,
//                    Role = nameof(Roles.User)
//                };
//                _context.UserDetails.Add(buyer);
//                await _context.SaveChangesAsync(cancellationToken);
//            }

//            // Create an invitation record
//            var invitation = new SellerBuyerInvitation
//            {
//                SellerId = seller.Id,
//                BuyerId = buyer.Id,
//                SellerPhoneNumber = request.SellerMobileNumber,
//                BuyerPhoneNumber = request.BuyerMobileNumber,
//                InvitationLink = GenerateInvitationLink(seller.Id, buyer.Id),
//                Status = nameof(ContractStatus.Pending),
//                ContractId = request.ContractId
//            };

//            _context.SellerBuyerInvitations.Add(invitation);

//            // Update the contract status to "Pending"
//            contract.Status = nameof(ContractStatus.Pending);
//            _context.ContractDetails.Update(contract);

//            // Save all changes
//            await _context.SaveChangesAsync(cancellationToken);

//            return invitation.InvitationLink;
//        }
//    }
//}
