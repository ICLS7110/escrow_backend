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
        disputeGroup.MapPost("/{disputeId:int}/assign-arbitrator", AssignArbitrator).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin), nameof(Roles.User)));
        disputeGroup.MapPost("/update-status", UpdateDisputeStatus).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin), nameof(Roles.User)));


        disputeGroup.MapPost("/escrow-decision", MakeEscrowDecision).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin), nameof(Roles.User)));

        // 🆕 User-facing endpoint
        disputeGroup.MapPost("/create", CreateDispute).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin), nameof(Roles.User))); ; // ⬅️ New user-facing route
    }

    [Authorize]
    public async Task<IResult> GetAllDisputes(ISender sender, [AsParameters] GetDisputesQuery request)
    {
        var result = await sender.Send(request);
        return result.Items.Any()
            ? TypedResults.Ok(Result<PaginatedList<DisputeDTO>>.Success(StatusCodes.Status200OK, "Success", result))
            : TypedResults.NotFound(Result<object>.Failure(StatusCodes.Status404NotFound, "No disputes found."));
    }


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
    public async Task<IResult> CreateDispute(ISender sender, [FromBody] CreateDisputeCommand command)
    {
        var result = await sender.Send(command);

        if (result.Status is >= StatusCodes.Status200OK)
        {
            return TypedResults.Ok(Result<object>.Success(StatusCodes.Status201Created, "Dispute created successfully.", new { DisputeId = result.Data }));
        }

        return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, result.Message ?? "Failed to create dispute."));
    }


    [Authorize]
    public async Task<IResult> UpdateDisputeStatusV2(
    ISender sender,
    [FromBody] UpdateStatusCommand command)
    {
        var result = await sender.Send(command);
        return TypedResults.Ok(result);
    }


}

