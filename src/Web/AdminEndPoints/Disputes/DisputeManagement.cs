using Escrow.Api.Application.Common.Constants;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.Disputes.Commands;
using Escrow.Api.Application.Disputes.Queries;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Escrow.Api.Web.AdminEndPoints.Disputes;


public class DisputeManagement : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var disputeGroup = app.MapGroup(this)
            .WithOpenApi();

        // Admin-only endpoints
        disputeGroup.MapGet("/", GetAllDisputes).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin), nameof(Roles.User)));
        //disputeGroup.MapGet("/{disputeId:int}", GetDisputeById).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin), nameof(Roles.User)));
        disputeGroup.MapPost("/{disputeId:int}/assign-arbitrator", AssignArbitrator).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin), nameof(Roles.User)));
        disputeGroup.MapPost("/update-status", UpdateDisputeStatus).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin), nameof(Roles.User)));


        disputeGroup.MapPost("/escrow-decision", MakeEscrowDecision).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin), nameof(Roles.User)));

        // 🆕 User-facing endpoint
        disputeGroup.MapPost("/create", CreateDispute).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin),nameof(Roles.User))); ; // ⬅️ New user-facing route



    //    disputeGroup.MapPost("/update-status-v2", UpdateDisputeStatusV2)
    //.WithName("UpdateDisputeStatusV2")
    //.WithOpenApi()
    //.RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin), nameof(Roles.User)));

    }


    [Authorize]
    public async Task<IResult> GetAllDisputes(ISender sender,[AsParameters] GetDisputesQuery request,IHttpContextAccessor _httpContextAccessor)
    {
        var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        var result = await sender.Send(request);

        if (result.Items.Any())
        {
            var successMessage = AppMessages.Get("DisputesRetrieved", language);
            return TypedResults.Ok(Result<PaginatedList<DisputeDTO>>.Success(StatusCodes.Status200OK, successMessage, result));
        }

        var notFoundMessage = AppMessages.Get("NoDisputesFound", language);
        return TypedResults.NotFound(Result<object>.Failure(StatusCodes.Status404NotFound, notFoundMessage));
    }

    //[Authorize]
    //public async Task<IResult> GetAllDisputes(ISender sender, [AsParameters] GetDisputesQuery request)
    //{
    //    var result = await sender.Send(request);
    //    return result.Items.Any()
    //        ? TypedResults.Ok(Result<PaginatedList<DisputeDTO>>.Success(StatusCodes.Status200OK, "Success", result))
    //        : TypedResults.NotFound(Result<object>.Failure(StatusCodes.Status404NotFound, "No disputes found."));
    //}

    //[Authorize]
    //public async Task<IResult> GetDisputeById(ISender sender, int disputeId)
    //{
    //    var result = await sender.Send(new GetDisputeByIdQuery(disputeId));
    //    return result?.Data != null
    //        ? TypedResults.Ok(result)
    //        : TypedResults.NotFound(Result<object>.Failure(StatusCodes.Status404NotFound, "Dispute not found."));
    //}

    [Authorize]
    public async Task<IResult> AssignArbitrator(ISender sender, AssignArbitratorCommand command)
    {
        var result = await sender.Send(command);
        return TypedResults.Ok(result);
    }

    [Authorize]
    public async Task<IResult> UpdateDisputeStatus(
    ISender sender,
    [FromBody] UpdateStatusCommand command)
    {
        var result = await sender.Send(command);
        return TypedResults.Ok(result);
    }


    [Authorize]
    public async Task<IResult> MakeEscrowDecision(ISender sender, [FromBody] EscrowDecisionCommand command)
    {
        var result = await sender.Send(command);
        return TypedResults.Ok(result);
    }


    [Authorize]
    public async Task<IResult> CreateDispute(ISender sender,[FromBody] CreateDisputeCommand command,IHttpContextAccessor _httpContextAccessor)
    {
        var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        var result = await sender.Send(command);

        if (result.Status is >= StatusCodes.Status200OK)
        {
            var successMessage = AppMessages.Get("DisputeCreated", language);
            return TypedResults.Ok(Result<object>.Success(StatusCodes.Status201Created, successMessage, new { DisputeId = result.Data }));
        }

        var failureMessage = AppMessages.Get(result.Message ?? "DisputeCreationFailed", language);
        return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, failureMessage));
    }








    //[Authorize]
    //public async Task<IResult> CreateDispute(ISender sender, [FromBody] CreateDisputeCommand command)
    //{
    //    var result = await sender.Send(command);

    //    if (result.Status is >= StatusCodes.Status200OK)
    //    {
    //        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status201Created, "Dispute created successfully.", new { DisputeId = result.Data }));
    //    }

    //    return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, result.Message ?? "Failed to create dispute."));
    //}


    [Authorize]
    public async Task<IResult> UpdateDisputeStatusV2(
    ISender sender,
    [FromBody] UpdateStatusCommand command)
    {
        var result = await sender.Send(command);
        return TypedResults.Ok(result);
    }


}



//public class DisputeManagement : EndpointGroupBase
//{
//    public override void Map(WebApplication app)
//    {
//        var disputeGroup = app.MapGroup(this)
//            .RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin)))
//            .WithOpenApi();

//        disputeGroup.MapGet("/", GetAllDisputes);
//        disputeGroup.MapGet("/{disputeId:int}", GetDisputeById);
//        disputeGroup.MapPost("/{disputeId:int}/assign-arbitrator", AssignArbitrator);
//        disputeGroup.MapPost("/update-status", UpdateDisputeStatus);

//        disputeGroup.MapPost("/escrow-decision", MakeEscrowDecision);
//    }

//    [Authorize]
//    public async Task<IResult> GetAllDisputes(ISender sender, [AsParameters] GetDisputesQuery request)
//    {
//        var result = await sender.Send(request);
//        return result.Items.Any()
//            ? TypedResults.Ok(Result<PaginatedList<DisputeDTO>>.Success(StatusCodes.Status200OK, "Success", result))
//            : TypedResults.NotFound(Result<object>.Failure(StatusCodes.Status404NotFound, "No disputes found."));
//    }

//    [Authorize]
//    public async Task<IResult> GetDisputeById(ISender sender, int disputeId)
//    {
//        var result = await sender.Send(new GetDisputeByIdQuery(disputeId));
//        return result?.Data != null
//            ? TypedResults.Ok(result)
//            : TypedResults.NotFound(Result<object>.Failure(StatusCodes.Status404NotFound, "Dispute not found."));
//    }

//    [Authorize]
//    public async Task<IResult> AssignArbitrator(ISender sender, AssignArbitratorCommand command)
//    {
//        var result = await sender.Send(command);
//        return TypedResults.Ok(result);
//    }

//    [Authorize]
//    public async Task<IResult> UpdateDisputeStatus(ISender sender, [FromBody] UpdateDisputeStatusCommand command)
//    {
//        var result = await sender.Send(command);
//        return TypedResults.Ok(result);
//    }


//    [Authorize]
//    public async Task<IResult> MakeEscrowDecision(ISender sender, [FromBody] EscrowDecisionCommand command)
//    {
//        var result = await sender.Send(command);
//        return TypedResults.Ok(result);
//    }
//}
