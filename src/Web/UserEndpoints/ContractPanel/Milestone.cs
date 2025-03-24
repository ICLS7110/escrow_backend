using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models.ContractDTOs;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Application.ContractPanel.MilestoneCommands;
using Escrow.Api.Application.ContractPanel.MilestoneQueries;
using Microsoft.AspNetCore.Authorization;
using Escrow.Api.Application;
using Escrow.Api.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Escrow.Api.Domain.Enums;

namespace Escrow.Api.Web.UserEndpoints.ContractPanel;

public class Milestone : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var userGroup = app.MapGroup(this)
        .RequireAuthorization() // Enable OpenIddict authorization
        .WithOpenApi()
        .AddEndpointFilter(async (context, next) =>
        {
            // Optional: Add custom authorization logic if needed
            return await next(context);
        });

        userGroup.MapGet("/", GetMilestoneDetails).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.User)));
        userGroup.MapPost("/", CreateMiliestone).RequireAuthorization(p => p.RequireRole(nameof(Roles.User)));
        userGroup.MapPost("/update", UpdateMilestone).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.User)));
    }

    [Authorize]
    public async Task<IResult> GetMilestoneDetails(ISender sender, IJwtService jwtService, int? contractId)
    {
        var query = new GetMilestoneQuery { Id = jwtService.GetUserId().ToInt(), ContractId = contractId, PageNumber = 1, PageSize = 10 };
        var result = await sender.Send(query);
        return TypedResults.Ok(Result<PaginatedList<MileStoneDTO>>.Success(StatusCodes.Status200OK, "Success", result));
    }

    [Authorize]
    public async Task<IResult> CreateMiliestone(ISender sender, CreateMilestoneCommand command)
    {
        await sender.Send(command);
        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status200OK, "Milestone Created Successfully.", new()));
    }

    [Authorize]
    public async Task<IResult> UpdateMilestone(ISender sender, EditMilestoneCommand command)
    {
        await sender.Send(command);
        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status204NoContent, "Milestone details updated successfully.", new()));
    }
}
