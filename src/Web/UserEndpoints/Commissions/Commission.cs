using Escrow.Api.Application.Commissions.Commands;
using Escrow.Api.Application.Commissions.Queries;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Escrow.Api.Web.UserEndpoints.Commissions;

public class CommissionMaster : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var userGroup = app.MapGroup(this)
            .RequireAuthorization()
            .WithOpenApi();

        userGroup.MapGet("/", GetCommissionRate)
            .RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin), nameof(Roles.User)));

        userGroup.MapPost("/upsert", UpsertCommissionRate)
            .RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin), nameof(Roles.User)));
    }

    private bool IsAdmin(IHttpContextAccessor httpContextAccessor) =>
        httpContextAccessor.HttpContext?.User.IsInRole(nameof(Roles.Admin)) ?? false;

    public async Task<IResult> GetCommissionRate([FromServices] ISender sender, [FromQuery] int? id)
    {
        var result = await sender.Send(new GetCommissionRateQuery { Id = id });

        if (result?.Data == null || !result.Data.Any())
        {
            return TypedResults.NotFound(Result<List<CommissionDTO>>.Failure(
                StatusCodes.Status404NotFound, "No commission data found."));
        }

        return TypedResults.Ok(result);
    }

    public async Task<IResult> UpsertCommissionRate(
        [FromServices] ISender sender,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromBody] UpsertCommissionRateCommand command)
    {

        if (!IsAdmin(httpContextAccessor))
            return TypedResults.Forbid();

        var result = await sender.Send(command);

        if (result.Status == StatusCodes.Status400BadRequest)
        {
            return TypedResults.BadRequest(Result<object>.Failure(
                StatusCodes.Status400BadRequest, result.Message ?? "Commission rate upsert failed."));
        }

        return TypedResults.Ok(result);
    }
}


























//using Escrow.Api.Application.Commissions.Commands;
//using Escrow.Api.Application.Commissions.Queries;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.Common.Models;
//using Escrow.Api.Application.DTOs;
//using Escrow.Api.Domain.Enums;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace Escrow.Api.Web.UserEndpoints.Commissions;

//public class CommissionMaster : EndpointGroupBase
//{
//    public override void Map(WebApplication app)
//    {
//        var userGroup = app.MapGroup(this)
//            .RequireAuthorization()
//            .WithOpenApi();

//        userGroup.MapGet("/", GetCommissionRate)
//            .RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin), nameof(Roles.User)));

//        userGroup.MapPost("/update", UpdateCommissionRate)
//            .RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin)));
//    }

//    private bool IsAdmin(IHttpContextAccessor httpContextAccessor) =>
//        httpContextAccessor.HttpContext?.User.IsInRole(nameof(Roles.Admin)) ?? false;

//    public async Task<IResult> GetCommissionRate([FromServices] ISender sender, [FromQuery] int? id)
//    {
//        var result = await sender.Send(new GetCommissionRateQuery { Id = id });

//        if (result?.Data == null || !result.Data.Any())
//        {
//            return TypedResults.NotFound(Result<List<CommissionDTO>>.Failure(
//                StatusCodes.Status404NotFound, "No commission data found."));
//        }

//        return TypedResults.Ok(result);
//    }

//    public async Task<IResult> UpdateCommissionRate(
//        [FromServices] ISender sender,
//        [FromServices] IHttpContextAccessor httpContextAccessor,
//        [FromBody] UpdateCommissionRateCommand command)
//    {
//        if (!IsAdmin(httpContextAccessor))
//            return TypedResults.Forbid();

//        var result = await sender.Send(command);

//        if (result?.Data != null)
//        {
//            return TypedResults.BadRequest(Result<object>.Failure(
//                StatusCodes.Status400BadRequest, "Commission rate update failed."));
//        }

//        return TypedResults.Ok(Result<object>.Success(
//            StatusCodes.Status200OK, "Commission rate updated successfully.", new()));
//    }
//}
