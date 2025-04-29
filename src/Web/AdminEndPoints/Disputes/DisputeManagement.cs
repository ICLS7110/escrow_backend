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
            .RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin)))
            .WithOpenApi();

        disputeGroup.MapGet("/", GetAllDisputes);
        disputeGroup.MapGet("/{disputeId:int}", GetDisputeById);
        disputeGroup.MapPost("/{disputeId:int}/assign-arbitrator", AssignArbitrator);
        disputeGroup.MapPost("/update-status", UpdateDisputeStatus);

        disputeGroup.MapPost("/escrow-decision", MakeEscrowDecision);
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
    public async Task<IResult> GetDisputeById(ISender sender, int disputeId)
    {
        var result = await sender.Send(new GetDisputeByIdQuery(disputeId));
        return result?.Data != null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound(Result<object>.Failure(StatusCodes.Status404NotFound, "Dispute not found."));
    }

    [Authorize]
    public async Task<IResult> AssignArbitrator(ISender sender, AssignArbitratorCommand command)
    {
        var result = await sender.Send(command);
        return TypedResults.Ok(result);
    }

    [Authorize]
    public async Task<IResult> UpdateDisputeStatus(ISender sender, [FromBody] UpdateDisputeStatusCommand command)
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
}
