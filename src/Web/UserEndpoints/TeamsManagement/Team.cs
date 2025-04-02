using System.Diagnostics.CodeAnalysis;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Application.TeamsManagement.Commands;
using Escrow.Api.Application.TeamsManagement.Queries;
using Escrow.Api.Domain.Enums;
using Escrow.Api.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace Escrow.Api.Web.UserEndpoints.TeamsManagement;

public class Team : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {

        var adminGroup = app.MapGroup(this)
            .RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin), nameof(Roles.User))) // Only Admins & Users
            .WithOpenApi();

        adminGroup.MapGet("/", GetAllTeams);
        adminGroup.MapPost("/create", CreateTeam);
        adminGroup.MapPut("/update", UpdateTeam);
        adminGroup.MapPut("/update-status", UpdateTeamStatus);
        adminGroup.MapDelete("/delete", DeleteTeam);
    }

    /// <summary>
    /// Retrieves all teams or a specific team by ID or UserId.
    /// Supports pagination and filtering by RoleType, IsActive, and LastDays.
    /// </summary>
    [Authorize]
    public async Task<IResult> GetAllTeams(ISender sender, int pageNumber = 1, int pageSize = 10, string? roleType = null, bool? isActive = null, int? lastDays = null)
    {
        var result = await sender.Send(new GetAllTeamsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            RoleType = roleType,
            IsActive = isActive,
            LastDays = lastDays
        });

        if (result == null || result.Data == null)
        {
            return TypedResults.NotFound(Result<object>.Failure(StatusCodes.Status200OK, "No teams found."));
        }

        return TypedResults.Ok(Result<PaginatedList<TeamDTO>>.Success(
            StatusCodes.Status200OK,
            "Teams retrieved successfully.",
            result.Data
        ));
    } 

    /// <summary>
    /// Creates a new team with UserId from JWT.
    /// </summary>
    [Authorize]
    public async Task<IResult> CreateTeam(ISender sender, CreateTeamCommand command)
    {
        if (command == null)
        {
            return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, "Invalid request payload."));
        }

        try
        {
            var result = await sender.Send(command);

            if (result.Status != StatusCodes.Status200OK) // Check if the operation was unsuccessful
            {
                return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, result.Message ?? "Team creation failed."));
            }

            return TypedResults.Ok(result);
        }
        catch (Exception ex)
        {
            // Log the error (replace with actual logging if available)
            Console.WriteLine($"Error creating team: {ex.Message}");

            return TypedResults.Json(
                Result<object>.Failure(StatusCodes.Status500InternalServerError, "An unexpected server error occurred."),
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Updates an existing team. UserId is retrieved from JWT.
    /// </summary>
    [Authorize]
    public async Task<IResult> UpdateTeam(ISender sender, UpdateTeamCommand command)
    {
        var result = await sender.Send(command);

        if (result.Status == StatusCodes.Status400BadRequest)
        {
            return TypedResults.BadRequest(result);
        }

        if (result.Status == StatusCodes.Status404NotFound)
        {
            return TypedResults.NotFound(result);
        }

        return TypedResults.Ok(result);
    }

    [Authorize]
    public async Task<IResult> UpdateTeamStatus(ISender sender, UpdateTeamStatusCommand command)
    {
        if (command == null || command.TeamId <= 0)
        {
            return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, "Invalid team ID or request payload."));
        }

        var result = await sender.Send(command);



        if (result.Status == StatusCodes.Status404NotFound)
        {
            return TypedResults.NotFound(Result<object>.Failure(result.Status, result.Message ?? "Team not found or status update failed."));
        }

        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status200OK, "Team status updated successfully."));
    }

    [Authorize]
    public async Task<IResult> DeleteTeam(ISender sender, int teamId)
    {
        if (teamId <= 0)
        {
            return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, "Invalid team ID."));
        }
        var result = await sender.Send(new DeleteTeamCommand { TeamId = teamId });


        if (result.Status == StatusCodes.Status404NotFound)
        {
            return TypedResults.NotFound(Result<object>.Failure(result.Status, result.Message ?? "Team not found"));
        }

        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status200OK, "Team Deleted successfully."));
    }

}
