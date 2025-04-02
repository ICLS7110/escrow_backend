using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.Common.Models.ContactDTO;
using Escrow.Api.Application.ContactUsPanel.Commands;
using Escrow.Api.Application.ContactUsPanel.Queries;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Escrow.Api.Web.UserEndpoints.ContactUsPanel;

public class ContactUs : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var contactUsGroup = app.MapGroup(this)
            .RequireAuthorization()
            .WithOpenApi()
            .AddEndpointFilter(async (context, next) => await next(context));

        contactUsGroup.MapPost("/send", SendContactMessage)
                      .RequireAuthorization(policy => policy.RequireRole(nameof(Roles.User)));

        contactUsGroup.MapGet("/messages", GetContactMessages)
                      .RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin)));
    }

    [Authorize]
    public async Task<IResult> SendContactMessage(ISender sender, [FromBody] SendContactMessageCommand request)
    {
        if (request == null)
            return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, "Invalid request data."));

        var result = await sender.Send(request);

        if (result.Status != StatusCodes.Status200OK)  // ✅ Using Status instead of IsSuccess
            return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, result.Message));

        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status200OK, "Contact message sent successfully."));
    }

    [Authorize(Roles = nameof(Roles.Admin))]
    public async Task<IResult> GetContactMessages(ISender sender, [FromQuery] int? pageNumber = 1, [FromQuery] int? pageSize = 10)
    {
        var query = new GetContactMessagesQuery
        {
            PageNumber = pageNumber ?? 1,
            PageSize = pageSize ?? 10
        };

        var result = await sender.Send(query);

        if (result == null || result.Items == null || !result.Items.Any())
            return TypedResults.NotFound(Result<PaginatedList<ContactUsDTO>>.Failure(StatusCodes.Status404NotFound, "No contact messages found."));

        return TypedResults.Ok(Result<PaginatedList<ContactUsDTO>>.Success(StatusCodes.Status200OK, "Success", result));
    }
}
