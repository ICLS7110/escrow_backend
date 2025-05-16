using Escrow.Api.Application.Commissions.Commands;
using Escrow.Api.Application.Commissions.Queries;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Escrow.Api.Web.UserEndpoints.Commissions;

public class CommissionMaster : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var userGroup = app.MapGroup(this)
        .RequireAuthorization()
        .WithOpenApi()
        .AddEndpointFilter(async (context, next) => await next(context));

        userGroup.MapGet("/", GetCommissionRate).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin),nameof(Roles.User)));
        userGroup.MapPut("/update", UpdateCommissionRate).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin), nameof(Roles.User)));
    }

    private bool IsAdmin(IHttpContextAccessor httpContextAccessor) =>
        httpContextAccessor.HttpContext?.User.IsInRole(nameof(Roles.Admin)) ?? false;

    [Authorize]
    public async Task<IResult> GetCommissionRate(ISender sender)
    {
        var result = await sender.Send(new GetCommissionRateQuery());

        if (result == null || result.Data == null)
            return TypedResults.NotFound(Result<List<CommissionDTO>>.Failure(StatusCodes.Status404NotFound, "No commission data found."));

        return TypedResults.Ok(result);
    }

    [Authorize]
    public async Task<IResult> UpdateCommissionRate(ISender sender, IJwtService jwtService, IHttpContextAccessor httpContextAccessor, UpdateCommissionRateCommand command)
    {
        if (!IsAdmin(httpContextAccessor))
            return TypedResults.Forbid();

        var result = await sender.Send(command);
        if (result.Data == 0)
            return TypedResults.NotFound(Result<object>.Failure(StatusCodes.Status404NotFound, "Commission rate update failed."));

        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status200OK, "Commission rate updated successfully.", new()));
    }
}
