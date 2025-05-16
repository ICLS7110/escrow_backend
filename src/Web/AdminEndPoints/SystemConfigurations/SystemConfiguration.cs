using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.SystemConfigurations.Commands;
using Escrow.Api.Application.SystemConfigurations.Queries;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Escrow.Api.Application.Common.Models.SystemConfiguration;
using Escrow.Api.Application.Common.Constants;

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
    public async Task<IResult> GetAllConfigurations(
    ISender sender,
    string? key,
    IHttpContextAccessor _httpContextAccessor)
    {
        var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        var result = await sender.Send(new GetAllSystemConfigurationsQuery { Key = key });

        if (result.Data == null || !result.Data.Any())
        {
            var message = AppMessages.Get("NoConfigurationsFound", language);
            return TypedResults.NotFound(Result<object>.Failure(StatusCodes.Status404NotFound, message));
        }

        var successMessage = AppMessages.Get("ConfigurationsRetrieved", language);
        return TypedResults.Ok(Result<List<SystemConfigurationDTO>>.Success(StatusCodes.Status200OK, successMessage, result.Data));
    }

    [Authorize]
    public async Task<IResult> UpsertConfiguration(
        ISender sender,
        [FromBody] UpsertSystemConfigurationCommand command,
        IHttpContextAccessor _httpContextAccessor)
    {
        var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        var result = await sender.Send(command);

        if (result.Status >= StatusCodes.Status200OK)
        {
            var message = AppMessages.Get("ConfigurationUpdated", language);
            return TypedResults.Ok(Result<object>.Success(StatusCodes.Status200OK, message, new { Key = command.Key, value = command.Value }));
        }

        var failureMessage = AppMessages.Get(result.Message ?? "ConfigurationUpdateFailed", language);
        return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, failureMessage));
    }




    //[Authorize]
    //public async Task<IResult> GetAllConfigurations(ISender sender, string? key)
    //{
    //    var result = await sender.Send(new GetAllSystemConfigurationsQuery { Key = key });

    //    if (result.Data == null || !result.Data.Any())
    //    {
    //        return TypedResults.NotFound(Result<object>.Failure(StatusCodes.Status404NotFound, "No configurations found."));
    //    }

    //    return TypedResults.Ok(Result<List<SystemConfigurationDTO>>.Success(StatusCodes.Status200OK, "Success", result.Data));
    //}

    //[Authorize]
    //public async Task<IResult> UpsertConfiguration(ISender sender, [FromBody] UpsertSystemConfigurationCommand command)
    //{
    //    var result = await sender.Send(command);

    //    return result.Status >= StatusCodes.Status200OK
    //        ? TypedResults.Ok(Result<object>.Success(StatusCodes.Status200OK, "Configuration updated successfully.", new { Key = command.Key, value = command.Value }))
    //        : TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, result.Message ?? "Failed to update configuration."));
    //}
}
