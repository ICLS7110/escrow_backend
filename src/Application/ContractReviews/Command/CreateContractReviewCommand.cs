using System;
using System.Linq;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Constants;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.ContractReviews;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Escrow.Api.Application.Common.Helpers;

namespace Escrow.Api.Application.ContractReviews.Command
{
    public record CreateContractReviewCommand : IRequest<Result<object>>
    {
        public int ContractId { get; set; }
        public string Rating { get; set; } = string.Empty;
        public string ReviewText { get; set; } = string.Empty;
    }

    public class CreateContractReviewCommandHandler : IRequestHandler<CreateContractReviewCommand, Result<object>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;

        public CreateContractReviewCommandHandler(IApplicationDbContext context, IJwtService jwtService, IHttpContextAccessor httpContextAccessor, IEmailService emailService)
        {
            _context = context;
            _jwtService = jwtService;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
        }

        public async Task<Result<object>> Handle(CreateContractReviewCommand request, CancellationToken cancellationToken)
        {
            // Get language for localized messages
            var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

            // Validate contract existence
            var contract = await _context.ContractDetails
                .Where(c => c.Id == request.ContractId)
                .FirstOrDefaultAsync(cancellationToken);

            if (contract == null)
            {
                return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("ContractNotFound", language));
            }

            // Fetch buyer and seller IDs based on their mobile numbers
            var buyer = await _context.UserDetails.FirstOrDefaultAsync(u => u.PhoneNumber == contract.BuyerMobile, cancellationToken);
            var seller = await _context.UserDetails.FirstOrDefaultAsync(u => u.PhoneNumber == contract.SellerMobile, cancellationToken);

            if (buyer == null || seller == null)
            {
                return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("BuyerAndSellerNotFound", language));
            }

            // Extract user ID from JWT token
            if (!int.TryParse(_jwtService.GetUserId(), out int userId))
            {
                return Result<object>.Failure(StatusCodes.Status401Unauthorized, AppMessages.Get("InvalidUserID", language));
            }

            int reviewerId, revieweeId;

            // Determine reviewer and reviewee based on current user
            if (userId == buyer.Id)
            {
                reviewerId = buyer.Id;
                revieweeId = seller.Id;
            }
            else if (userId == seller.Id)
            {
                reviewerId = seller.Id;
                revieweeId = buyer.Id;
            }
            else
            {
                return Result<object>.Failure(StatusCodes.Status403Forbidden, AppMessages.Get("UnauthorizedToReviewContract", language));
            }

            // Create the review entity
            var review = new ContractReview
            {
                ContractId = request.ContractId,
                ReviewerId = reviewerId,
                RevieweeId = revieweeId,
                Rating = request.Rating,
                ReviewText = request.ReviewText,
                Created = DateTime.UtcNow,
                CreatedBy = userId.ToString(),
                LastModified = DateTime.UtcNow,
                LastModifiedBy = userId.ToString()
            };

            _context.ContractReviews.Add(review);

            // Update contract status to Completed
            contract.Status = nameof(ContractStatus.Completed);
            _context.ContractDetails.Update(contract);

            await _context.SaveChangesAsync(cancellationToken);


            // Direct email content
            var emailSubject = "New Contract Review Submitted";
            var emailBody = $"A new review has been submitted for your contract.\n\n" +
                            $"Rating: {request.Rating}\n" +
                            $"Review: {request.ReviewText}\n\n" +
                            "Thank you for your feedback!";

            // Send email to both buyer and seller
            if (!string.IsNullOrEmpty(buyer?.EmailAddress) && !string.IsNullOrEmpty(buyer?.FullName))
            {
                await _emailService.SendEmailAsync(buyer.EmailAddress, emailSubject, buyer.FullName, emailBody);
            }

            if (!string.IsNullOrEmpty(seller?.EmailAddress) && !string.IsNullOrEmpty(seller?.FullName))
            {
                await _emailService.SendEmailAsync(seller.EmailAddress, emailSubject, seller.FullName, emailBody);
            }

            return Result<object>.Success(StatusCodes.Status200OK, AppMessages.Get("ReviewCreated", language), new { ReviewId = review.Id });
        }
    }
}









































//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Constants;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.DTOs;
//using Escrow.Api.Domain.Entities.ContractReviews;
//using Escrow.Api.Domain.Enums;
//using Microsoft.AspNetCore.Http;

//namespace Escrow.Api.Application.ContractReviews.Command;
//public record CreateContractReviewCommand : IRequest<Result<object>>
//{
//    public int ContractId { get; set; }
//    public string Rating { get; set; } = string.Empty;
//    public string ReviewText { get; set; } = string.Empty;
//}

//public class CreateContractReviewCommandHandler : IRequestHandler<CreateContractReviewCommand, Result<object>>
//{
//    private readonly IApplicationDbContext _context;
//    private readonly IJwtService _jwtService;

//    public CreateContractReviewCommandHandler(IApplicationDbContext context, IJwtService jwtService)
//    {
//        _context = context;
//        _jwtService = jwtService;
//    }

//    public async Task<Result<object>> Handle(CreateContractReviewCommand request, CancellationToken cancellationToken)
//    {
//        // Validate contract existence
//        var contract = await _context.ContractDetails
//            .Where(c => c.Id == request.ContractId)
//            .FirstOrDefaultAsync(cancellationToken);

//        if (contract == null)
//        {
//            return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.ContractNotFound);
//        }

//        // Fetch buyer and seller IDs based on their mobile numbers
//        var buyer = await _context.UserDetails.FirstOrDefaultAsync(u => u.PhoneNumber == contract.BuyerMobile, cancellationToken);
//        var seller = await _context.UserDetails.FirstOrDefaultAsync(u => u.PhoneNumber == contract.SellerMobile, cancellationToken);

//        if (buyer == null || seller == null)
//        {
//            return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.BuyerAndSellerNotFound);
//        }

//        // Extract user ID from JWT token
//        if (!int.TryParse(_jwtService.GetUserId(), out int userId))
//        {
//            return Result<object>.Failure(StatusCodes.Status401Unauthorized, AppMessages.InvaliduserID);
//        }

//        int reviewerId, revieweeId;

//        // Determine reviewer and reviewee based on current user
//        if (userId == buyer.Id)
//        {
//            reviewerId = buyer.Id;
//            revieweeId = seller.Id;
//        }
//        else if (userId == seller.Id)
//        {
//            reviewerId = seller.Id;
//            revieweeId = buyer.Id;
//        }
//        else
//        {
//            return Result<object>.Failure(StatusCodes.Status403Forbidden, AppMessages.UnauthorizedToReviewContract);
//        }

//        // Create the review entity
//        var review = new ContractReview
//        {
//            ContractId = request.ContractId,
//            ReviewerId = reviewerId,
//            RevieweeId = revieweeId,
//            Rating = request.Rating,
//            ReviewText = request.ReviewText,
//            Created = DateTime.UtcNow,
//            CreatedBy = userId.ToString(),
//            LastModified = DateTime.UtcNow,
//            LastModifiedBy = userId.ToString()
//        };

//        _context.ContractReviews.Add(review);

//        // Update contract status to Completed
//        contract.Status = nameof(ContractStatus.Completed);
//        _context.ContractDetails.Update(contract);

//        await _context.SaveChangesAsync(cancellationToken);

//        return Result<object>.Success(StatusCodes.Status200OK, AppMessages.ReviewCreated, new { ReviewId = review.Id });
//    }
//}
