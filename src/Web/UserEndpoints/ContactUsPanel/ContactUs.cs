using Escrow.Api.Application.Common.Constants;
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

        contactUsGroup.MapPost("/send", SendContactMessage).AllowAnonymous();


        contactUsGroup.MapGet("/messages", GetContactMessages)
                      .RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin)));
    }


    public async Task<IResult> SendContactMessage(
    ISender sender,
    IHttpContextAccessor httpContextAccessor,
    [FromBody] SendContactMessageCommand request)
    {
        var language = httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        if (request == null)
        {
            var message = AppMessages.Get("InvalidContactRequest", language);
            return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, message));
        }

        var result = await sender.Send(request);

        if (result.Status != StatusCodes.Status200OK)
        {
            var message = AppMessages.Get("ContactMessageFailed", language);
            return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, message));
        }

        var successMessage = AppMessages.Get("ContactMessageSent", language);
        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status200OK, successMessage));
    }

    [Authorize(Roles = nameof(Roles.Admin))]
    public async Task<IResult> GetContactMessages(
        ISender sender,
        IHttpContextAccessor httpContextAccessor,
        [FromQuery] int? pageNumber = 1,
        [FromQuery] int? pageSize = 10)
    {
        var language = httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        var query = new GetContactMessagesQuery
        {
            PageNumber = pageNumber ?? 1,
            PageSize = pageSize ?? 10
        };

        var result = await sender.Send(query);

        if (result == null || result.Items == null || !result.Items.Any())
        {
            var message = AppMessages.Get("NoContactMessagesFound", language);
            return TypedResults.NotFound(Result<PaginatedList<ContactUsDTO>>.Failure(StatusCodes.Status404NotFound, message));
        }

        var successMessage = AppMessages.Get("ContactMessagesRetrieved", language);
        return TypedResults.Ok(Result<PaginatedList<ContactUsDTO>>.Success(StatusCodes.Status200OK, successMessage, result));
    }










    //public async Task<IResult> SendContactMessage(ISender sender, IHttpContextAccessor httpContextAccessor, [FromBody] SendContactMessageCommand request)
    //{
    //    if (request == null)
    //        return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, "Invalid request data."));

    //    var language = httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

    //    var result = await sender.Send(request);

    //    if (result.Status != StatusCodes.Status200OK)
    //        return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, result.Message));

    //    return TypedResults.Ok(Result<object>.Success(StatusCodes.Status200OK, AppMessages.Get("ContactMessageSent", language)));
    //}


    ////public async Task<IResult> SendContactMessage(ISender sender, [FromBody] SendContactMessageCommand request)
    ////{
    ////    if (request == null)
    ////        return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, "Invalid request data."));

    ////    var result = await sender.Send(request);

    ////    if (result.Status != StatusCodes.Status200OK)  // ✅ Using Status instead of IsSuccess
    ////        return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, result.Message));

    ////    return TypedResults.Ok(Result<object>.Success(StatusCodes.Status200OK, "Contact message sent successfully."));
    ////}

    //[Authorize(Roles = nameof(Roles.Admin))]
    //public async Task<IResult> GetContactMessages(ISender sender, [FromQuery] int? pageNumber = 1, [FromQuery] int? pageSize = 10)
    //{
    //    var query = new GetContactMessagesQuery
    //    {
    //        PageNumber = pageNumber ?? 1,
    //        PageSize = pageSize ?? 10
    //    };

    //    var result = await sender.Send(query);

    //    if (result == null || result.Items == null || !result.Items.Any())
    //        return TypedResults.NotFound(Result<PaginatedList<ContactUsDTO>>.Failure(StatusCodes.Status404NotFound, "No contact messages found."));

    //    return TypedResults.Ok(Result<PaginatedList<ContactUsDTO>>.Success(StatusCodes.Status200OK, "Success", result));
    //}
}
