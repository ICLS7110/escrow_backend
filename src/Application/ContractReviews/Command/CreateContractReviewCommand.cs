using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.ContractReviews;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.ContractReviews.Command;
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

    public CreateContractReviewCommandHandler(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<Result<object>> Handle(CreateContractReviewCommand request, CancellationToken cancellationToken)
    {
        // Validate contract existence
        var contract = await _context.ContractDetails
            .Where(c => c.Id == request.ContractId)
            .FirstOrDefaultAsync(cancellationToken);

        if (contract == null)
        {
            return Result<object>.Failure(StatusCodes.Status404NotFound, "Contract not found.");
        }

        // Fetch buyer and seller IDs based on their mobile numbers
        var buyer = await _context.UserDetails.FirstOrDefaultAsync(u => u.PhoneNumber == contract.BuyerMobile, cancellationToken);
        var seller = await _context.UserDetails.FirstOrDefaultAsync(u => u.PhoneNumber == contract.SellerMobile, cancellationToken);

        if (buyer == null || seller == null)
        {
            return Result<object>.Failure(StatusCodes.Status404NotFound, "Buyer or Seller not found.");
        }

        // Extract user ID from JWT token
        if (!int.TryParse(_jwtService.GetUserId(), out int userId))
        {
            return Result<object>.Failure(StatusCodes.Status401Unauthorized, "Invalid user ID.");
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
            return Result<object>.Failure(StatusCodes.Status403Forbidden, "You are not authorized to review this contract.");
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

        return Result<object>.Success(StatusCodes.Status200OK, "Review created successfully and contract marked as completed.", new { ReviewId = review.Id });
    }
}
