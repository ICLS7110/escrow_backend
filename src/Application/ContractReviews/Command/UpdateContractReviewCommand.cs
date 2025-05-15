using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Constants;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Escrow.Api.Application.Common.Helpers;

namespace Escrow.Api.Application.ContractReviews.Command
{
    public record UpdateContractReviewCommand : IRequest<Result<object>>
    {
        public int ReviewId { get; set; }
        public string Rating { get; set; } = string.Empty;
        public string ReviewText { get; set; } = string.Empty;
    }

    public class UpdateContractReviewCommandHandler : IRequestHandler<UpdateContractReviewCommand, Result<object>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;  // Injected Email Service

        public UpdateContractReviewCommandHandler(IApplicationDbContext context, IJwtService jwtService, IHttpContextAccessor httpContextAccessor,IEmailService emailService)
        {
            _context = context;
            _jwtService = jwtService;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;  // Initialize Email Service
        }

        public async Task<Result<object>> Handle(UpdateContractReviewCommand request, CancellationToken cancellationToken)
        {
            // Check if HttpContext is available and get the language
            var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English; // Default to English if not found

            if (!int.TryParse(_jwtService.GetUserId(), out int userId))
            {
                return Result<object>.Failure(StatusCodes.Status401Unauthorized, AppMessages.Get("InvalidUserID", language));
            }

            var review = await _context.ContractReviews.FindAsync(request.ReviewId);
            if (review == null)
            {
                return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("ReviewNotFound", language));
            }

            if (review.ReviewerId != userId)
            {
                return Result<object>.Failure(StatusCodes.Status403Forbidden, AppMessages.Get("UnauthorizedToUpdateReview", language));
            }

            review.Rating = request.Rating;
            review.ReviewText = request.ReviewText;
            review.LastModified = DateTime.UtcNow;
            review.LastModifiedBy = userId.ToString();

            _context.ContractReviews.Update(review);
            await _context.SaveChangesAsync(cancellationToken);

            var buyer = await _context.UserDetails.FirstOrDefaultAsync(u => u.Id == review.ReviewerId, cancellationToken);
            var seller = await _context.UserDetails.FirstOrDefaultAsync(u => u.Id == review.RevieweeId, cancellationToken);

            // Ensure buyer and seller details are available
            if (buyer != null && seller != null &&
                !string.IsNullOrEmpty(buyer.EmailAddress) && !string.IsNullOrEmpty(seller.EmailAddress))
            {
                var emailSubject = "Your Contract Review has been Updated";
                var emailBody = $"The review for your contract has been updated.\n\n" +
                                $"Rating: {request.Rating}\n" +
                                $"Review: {request.ReviewText}\n\n" +
                                "Thank you for your feedback!";

                // Send email to both buyer and seller
                if (!string.IsNullOrEmpty(buyer.EmailAddress) && !string.IsNullOrEmpty(buyer.FullName))
                {
                    await _emailService.SendEmailAsync(buyer.EmailAddress, emailSubject, buyer.FullName, emailBody);
                }

                if (!string.IsNullOrEmpty(seller.EmailAddress) && !string.IsNullOrEmpty(seller.FullName))
                {
                    await _emailService.SendEmailAsync(seller.EmailAddress, emailSubject, seller.FullName, emailBody);
                }
            }










            return Result<object>.Success(StatusCodes.Status200OK, AppMessages.Get("ReviewUpdated", language), new { ReviewId = review.Id });
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
//using Microsoft.AspNetCore.Http;

//namespace Escrow.Api.Application.ContractReviews.Command;

//public record UpdateContractReviewCommand : IRequest<Result<object>>
//{
//    public int ReviewId { get; set; }
//    public string Rating { get; set; } = string.Empty;
//    public string ReviewText { get; set; } = string.Empty;
//}

//public class UpdateContractReviewCommandHandler : IRequestHandler<UpdateContractReviewCommand, Result<object>>
//{
//    private readonly IApplicationDbContext _context;
//    private readonly IJwtService _jwtService;

//    public UpdateContractReviewCommandHandler(IApplicationDbContext context, IJwtService jwtService)
//    {
//        _context = context;
//        _jwtService = jwtService;
//    }

//    public async Task<Result<object>> Handle(UpdateContractReviewCommand request, CancellationToken cancellationToken)
//    {
//        if (!int.TryParse(_jwtService.GetUserId(), out int userId))
//        {
//            return Result<object>.Failure(StatusCodes.Status401Unauthorized, AppMessages.InvaliduserID);
//        }

//        var review = await _context.ContractReviews.FindAsync(request.ReviewId);
//        if (review == null)
//        {
//            return Result<object>.Failure(StatusCodes.Status404NotFound,AppMessages.Reviewnotfound);
//        }

//        if (review.ReviewerId != userId)
//        {
//            return Result<object>.Failure(StatusCodes.Status403Forbidden, AppMessages.UnauthorizedToUpdateReview);
//        }

//        review.Rating = request.Rating;
//        review.ReviewText = request.ReviewText;
//        review.LastModified = DateTime.UtcNow;
//        review.LastModifiedBy = userId.ToString();

//        _context.ContractReviews.Update(review);
//        await _context.SaveChangesAsync(cancellationToken);

//        return Result<object>.Success(StatusCodes.Status200OK, AppMessages.Reviewupdated, new { ReviewId = review.Id });
//    }
//}
