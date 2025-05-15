using Escrow.Api.Application.Common.Models.EmailTemplate;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Application.EmailTemplates.Commands;
using Escrow.Api.Application.EmailTemplates.Queries;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Escrow.Api.Web.AdminEndPoints.EmailTemplates;

public class EmailTemplate : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var emailGroup = app.MapGroup(this)
            .RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin)))
            .WithOpenApi();

        emailGroup.MapGet("/", GetAllEmailTemplates);
        emailGroup.MapGet("/{templateId:int}", GetEmailTemplateById);
        emailGroup.MapPost("/update", UpdateEmailTemplate);
        //emailGroup.MapPost("/update-status", UpdateEmailTemplateStatus);
    }

    [Authorize]
    public async Task<IResult> GetAllEmailTemplates(ISender sender, [AsParameters] GetEmailTemplatesQuery request)
    {
        var result = await sender.Send(request);
        return result.Data != null && result.Data.Any()
            ? TypedResults.Ok(result)
            : TypedResults.NotFound(Result<object>.Failure(StatusCodes.Status404NotFound, "No email templates found."));
    }


    [Authorize]
    public async Task<IResult> GetEmailTemplateById(ISender sender, int templateId)
    {
        var result = await sender.Send(new GetEmailTemplateByIdQuery(templateId));
        return result?.Data != null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound(Result<object>.Failure(StatusCodes.Status404NotFound, "Email template not found."));
    }

    [Authorize]
    public async Task<IResult> UpdateEmailTemplate(ISender sender, [FromBody] UpdateEmailTemplateCommand command)
    {
        var result = await sender.Send(command);
        return TypedResults.Ok(result);
    }

   
}
