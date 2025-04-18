using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.ContractReviews;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.ContractReviews.Queries;
public class GetContractReviewsQuery : IRequest<Result<List<ContractReviewDTO>>>
{
    public int? ReviewId { get; set; }
}

public class GetContractReviewsQueryHandler : IRequestHandler<GetContractReviewsQuery, Result<List<ContractReviewDTO>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public GetContractReviewsQueryHandler(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<Result<List<ContractReviewDTO>>> Handle(GetContractReviewsQuery request, CancellationToken cancellationToken)
    {
        var userId = _jwtService.GetUserId().ToString();

        var query = _context.ContractReviews.AsQueryable();

        if (request.ReviewId.HasValue)
        {
            query = query.Where(r => r.Id == request.ReviewId.Value);
        }
        else if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(r => r.RevieweeId == Convert.ToInt32(userId) || r.RevieweeId == Convert.ToInt32(userId) || r.CreatedBy == userId);
        }

        var reviews = await query.Select(r => new ContractReviewDTO
        {
            Id = r.Id,
            ContractId = r.ContractId,
            ReviewerId = r.ReviewerId,
            RevieweeId = r.RevieweeId,
            Rating = r.Rating,
            ReviewText = r.ReviewText,
        }).ToListAsync(cancellationToken);

        if (!reviews.Any())
            return Result<List<ContractReviewDTO>>.Failure(StatusCodes.Status404NotFound, "No reviews found.");

        return Result<List<ContractReviewDTO>>.Success(StatusCodes.Status200OK, "Reviews retrieved successfully.", reviews);
    }
}

