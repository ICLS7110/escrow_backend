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
using System.Text.Json;

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
            return await next(context);
        });

        userGroup.MapGet("/", GetMilestoneDetails).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.User)));
        userGroup.MapPost("/", CreateMilestone).RequireAuthorization(p => p.RequireRole(nameof(Roles.User)));
        userGroup.MapPost("/update", UpdateMilestones).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.User)));
    }

    [Authorize]
    public async Task<IResult> GetMilestoneDetails(ISender sender, IJwtService jwtService, int? contractId)
    {
        if (contractId == null || contractId <= 0)
        {
            return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, "Invalid contract ID."));
        }

        var query = new GetMilestoneQuery
        {
            Id = jwtService.GetUserId().ToInt(),
            ContractId = contractId,
            PageNumber = 1,
            PageSize = 10
        };

        var result = await sender.Send(query);

        return TypedResults.Ok(Result<PaginatedList<MileStoneDTO>>.Success(StatusCodes.Status200OK, "Success", result));
    }

    [Authorize]
    public async Task<IResult> CreateMilestone(ISender sender, CreateMilestoneCommand command)
    {
        // ✅ Validate request payload
        if (command?.MileStoneDetails == null || !command.MileStoneDetails.Any())
        {
            return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, "Invalid request data. Milestone details are required."));
        }


        try
        {
            var result = await sender.Send(command);

            // ✅ Ensure `result` is valid
            if (result == null)
            {
                return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, "No milestones were created or updated."));
            }

            // ✅ Success response
            return TypedResults.Ok(Result<object>.Success(StatusCodes.Status200OK, "Milestones created/updated successfully.", null));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] CreateMilestone: {ex}");
            return TypedResults.Json(
                Result<object>.Failure(StatusCodes.Status500InternalServerError, "An unexpected server error occurred."),
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }


    [Authorize]
    public async Task<IResult> UpdateMilestones(ISender sender, EditMilestoneCommand command)
    {
        if (command == null || command.Milestones == null || !command.Milestones.Any())
        {
            return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, "Invalid request data. Milestone updates are required."));
        }

        try
        {
            var result = await sender.Send(command);

            return result?.Data != null && result.Data.Any()
                ? TypedResults.Ok(Result<List<int>>.Success(StatusCodes.Status200OK, "Milestones updated successfully.", result.Data))
                : TypedResults.NotFound(Result<object>.Failure(StatusCodes.Status404NotFound, "No milestones were updated."));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] UpdateMilestones: {ex.Message}");
            return TypedResults.Json(
                Result<object>.Failure(StatusCodes.Status500InternalServerError, "An unexpected server error occurred."),
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}
