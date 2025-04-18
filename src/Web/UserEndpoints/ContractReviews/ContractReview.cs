using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.ContractReviews.Command;
using Escrow.Api.Application.ContractReviews.Queries;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Escrow.Api.Web.UserEndpoints.ContractReviews;

public class ContractReview : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var userGroup = app.MapGroup(this)
        .RequireAuthorization()
        .WithOpenApi()
        .AddEndpointFilter(async (context, next) => await next(context));

        userGroup.MapGet("/", GetReviews).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin), nameof(Roles.User)));
        userGroup.MapPost("/create", CreateReview).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.User)));
        userGroup.MapPut("/update", UpdateReview).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.User)));
    }

    private bool IsAdmin(IHttpContextAccessor httpContextAccessor) =>
        httpContextAccessor.HttpContext?.User.IsInRole(nameof(Roles.Admin)) ?? false;

    [Authorize]
    public async Task<IResult> GetReviews(ISender sender)
    {
        var result = await sender.Send(new GetContractReviewsQuery());

        if (result == null || result.Data == null)
            return TypedResults.NotFound(Result<IEnumerable<ContractReviewDTO>>.Failure(StatusCodes.Status404NotFound, "No reviews found."));

        return TypedResults.Ok(result);
    }

    [Authorize]
    public async Task<IResult> CreateReview(ISender sender, CreateContractReviewCommand command)
    {
        var result = await sender.Send(command);


        if (result == null)
            return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, "Failed to create review."));

        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status200OK, "Review created successfully.", new()));
    }

    [Authorize]
    public async Task<IResult> UpdateReview(ISender sender, IHttpContextAccessor httpContextAccessor, UpdateContractReviewCommand command)
    {
        if (!IsAdmin(httpContextAccessor))
            return TypedResults.Forbid();

        var result = await sender.Send(command);

        if (result == null)
            return TypedResults.NotFound(Result<object>.Failure(StatusCodes.Status404NotFound, "Review update failed."));

        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status200OK, "Review updated successfully.", new()));
    }
}
