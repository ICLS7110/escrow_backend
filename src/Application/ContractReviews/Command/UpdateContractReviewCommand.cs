using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.ContractReviews.Command;

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

    public UpdateContractReviewCommandHandler(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<Result<object>> Handle(UpdateContractReviewCommand request, CancellationToken cancellationToken)
    {
        if (!int.TryParse(_jwtService.GetUserId(), out int userId))
        {
            return Result<object>.Failure(StatusCodes.Status401Unauthorized, "Invalid user ID.");
        }

        var review = await _context.ContractReviews.FindAsync(request.ReviewId);
        if (review == null)
        {
            return Result<object>.Failure(StatusCodes.Status404NotFound, "Review not found.");
        }

        if (review.ReviewerId != userId)
        {
            return Result<object>.Failure(StatusCodes.Status403Forbidden, "You are not authorized to update this review.");
        }

        review.Rating = request.Rating;
        review.ReviewText = request.ReviewText;
        review.LastModified = DateTime.UtcNow;
        review.LastModifiedBy = userId.ToString();

        _context.ContractReviews.Update(review);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<object>.Success(StatusCodes.Status200OK, "Review updated successfully.", new { ReviewId = review.Id });
    }
}
