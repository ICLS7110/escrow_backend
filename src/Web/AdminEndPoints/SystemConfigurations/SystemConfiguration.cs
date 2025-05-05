using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.SystemConfigurations.Commands;
using Escrow.Api.Application.SystemConfigurations.Queries;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Escrow.Api.Application.Common.Models.SystemConfiguration;

namespace Escrow.Api.Web.AdminEndPoints.SystemConfigurations;

public class SystemConfiguration : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var configGroup = app.MapGroup(this)
            .WithOpenApi()
            .RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin)));

        configGroup.MapGet("/", GetAllConfigurations);
        configGroup.MapPost("/update", UpsertConfiguration);
    }

    [Authorize]
    public async Task<IResult> GetAllConfigurations(ISender sender, string? key)
    {
        var result = await sender.Send(new GetAllSystemConfigurationsQuery { Key = key });

        if (result.Data == null || !result.Data.Any())
        {
            return TypedResults.NotFound(Result<object>.Failure(StatusCodes.Status404NotFound, "No configurations found."));
        }

        return TypedResults.Ok(Result<List<SystemConfigurationDTO>>.Success(StatusCodes.Status200OK, "Success", result.Data));
    }

    [Authorize]
    public async Task<IResult> UpsertConfiguration(ISender sender, [FromBody] UpsertSystemConfigurationCommand command)
    {
        var result = await sender.Send(command);

        return result.Status >= StatusCodes.Status200OK
            ? TypedResults.Ok(Result<object>.Success(StatusCodes.Status200OK, "Configuration updated successfully.", new { Key = command.Key, value = command.Value }))
            : TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, result.Message ?? "Failed to update configuration."));
    }
}
