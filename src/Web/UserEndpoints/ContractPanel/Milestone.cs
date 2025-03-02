using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models.ContractDTOs;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.ContractPanel.MilestoneCommands;
using Escrow.Api.Application.ContractPanel.MilestoneQueries;
using Escrow.Api.Application.ResultHandler;
using Escrow.Api.Application.UserPanel.Commands.UpdateUser;
using Microsoft.AspNetCore.Authorization;
using Escrow.Api.Application;

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

        userGroup.MapGet("/", GetMilestoneDetails).RequireAuthorization(policy => policy.RequireRole("User"));
        userGroup.MapPost("/", CreateMiliestone).RequireAuthorization(p => p.RequireRole("User"));
        userGroup.MapPost("/update", UpdateMilestone).RequireAuthorization(policy => policy.RequireRole("User"));
    }

    [Authorize]
    public async Task<IResult> GetMilestoneDetails(ISender sender, IJwtService jwtService)
    {
        var query = new GetMilestoneQuery { Id = jwtService.GetUserId().ToInt(), PageNumber = 1, PageSize = 1 };
        var result = await sender.Send(query);
        return TypedResults.Ok(Result<PaginatedList<MileStoneDTO>>.Success(StatusCodes.Status200OK, "Success", result));
    }

    [Authorize]
    public async Task<IResult> CreateMiliestone(ISender sender, CreateMilestoneCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status204NoContent, "Contract Created Successfully.", new()));
    }

    [Authorize]
    public async Task<IResult> UpdateMilestone(ISender sender, EditMilestoneCommand command)
    {
        await sender.Send(command);
        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status204NoContent, "Contract details updated successfully.", new()));
    }
}
