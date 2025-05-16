using Amazon.S3.Model.Internal.MarshallTransformations;
using Escrow.Api.Application.Common.Constants;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Application.Notifications.Commands;
using Escrow.Api.Application.Notifications.Queries;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Escrow.Api.Web.UserEndpoints.Notifications;


public class NotificationEndpoint : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var notificationGroup = app.MapGroup(this)
            .RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin), nameof(Roles.User)))
            .WithOpenApi();

        notificationGroup.MapGet("/", GetAllNotifications);
        notificationGroup.MapGet("/{id}", GetNotificationById);
        notificationGroup.MapPost("/create", CreateNotification);
        notificationGroup.MapPut("/update", UpdateNotification);
        notificationGroup.MapDelete("/delete", DeleteNotification);
        notificationGroup.MapPost("/mark-read", MarkNotificationReadStatus);

        // manual notification
        notificationGroup.MapPost("/manual/create", CreateManualNotification);
        notificationGroup.MapGet("/manual", GetAllManualNotifications);


    }

    ///// <summary>
    ///// Retrieves all notifications with optional pagination.
    ///// </summary>
    //[Authorize]
    //public async Task<IResult> GetAllNotifications(ISender sender, string? filter, int pageNumber = 1, int pageSize = 10)
    //{
    //    if (pageNumber < 1 || pageSize < 1)
    //    {
    //        return TypedResults.BadRequest(Result<object>.Failure(
    //            StatusCodes.Status400BadRequest,
    //            "Page number and page size must be greater than zero."
    //        ));
    //    }

    //    var result = await sender.Send(new GetAllNotificationsQuery
    //    {
    //        PageNumber = pageNumber,
    //        Filter = filter,
    //        PageSize = pageSize
    //    });

    //    if (result == null)
    //    {
    //        return TypedResults.NotFound(Result<object>.Failure(
    //            StatusCodes.Status404NotFound,
    //            "No notifications found."
    //        ));
    //    }

    //    return TypedResults.Ok(Result<PaginatedList<NotificationDTO>>.Success(
    //        StatusCodes.Status200OK,
    //        "Notifications retrieved successfully.",
    //        result.Data
    //    ));
    //}


    ///// <summary>
    ///// Retrieves a notification by ID.
    ///// </summary>
    //[Authorize]
    //public async Task<IResult> GetNotificationById(ISender sender, int id)
    //{
    //    if (id <= 0)
    //    {
    //        return TypedResults.BadRequest(Result<object>.Failure(
    //            StatusCodes.Status400BadRequest,
    //            "Invalid notification ID. ID must be greater than zero."
    //        ));
    //    }

    //    var result = await sender.Send(new GetNotificationByIdQuery { Id = id });

    //    if (result == null || result.Data == null)
    //    {
    //        return TypedResults.NotFound(Result<object>.Failure(
    //            StatusCodes.Status404NotFound,
    //            "Notification not found."
    //        ));
    //    }

    //    return TypedResults.Ok(Result<NotificationDTO>.Success(
    //        StatusCodes.Status200OK,
    //        "Notification retrieved successfully.",
    //        result.Data
    //    ));
    //}

    //[Authorize]
    //public async Task<IResult> CreateNotification(ISender sender, CreateNotificationCommand command)
    //{
    //    try
    //    {

    //        var result = await sender.Send(command);
    //        if (result.Status == StatusCodes.Status400BadRequest)
    //        {
    //            return TypedResults.BadRequest(result);
    //        }

    //        if (result.Status == StatusCodes.Status404NotFound)
    //        {
    //            return TypedResults.NotFound(result);
    //        }

    //        return TypedResults.Ok(result);

    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"CreateNotification Exception: {ex.Message} | StackTrace: {ex.StackTrace}");
    //        return TypedResults.Json(
    //            Result<object>.Failure(StatusCodes.Status500InternalServerError, "An unexpected error occurred."),
    //            statusCode: StatusCodes.Status500InternalServerError
    //        );
    //    }
    //}




    ///// <summary>
    ///// Manual Notification
    ///// </summary>
    ///// <param name="sender"></param>
    ///// <param name="command"></param>
    ///// <returns></returns>

    //[Authorize(Roles = nameof(Roles.Admin))]
    //public async Task<IResult> CreateManualNotification(
    //ISender sender,
    //[FromBody] CreateManualNotificationCommand command)
    //{
    //    try
    //    {
    //        var result = await sender.Send(command);

    //        if (result.Status == StatusCodes.Status400BadRequest)
    //            return TypedResults.BadRequest(result);

    //        return TypedResults.Ok(result);
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"CreateManualNotification Exception: {ex.Message}");
    //        return TypedResults.Json(
    //            Result<object>.Failure(StatusCodes.Status500InternalServerError, "An unexpected error occurred."),
    //            statusCode: StatusCodes.Status500InternalServerError
    //        );
    //    }
    //}


    //[Authorize]
    //public async Task<IResult> GetAllManualNotifications(ISender sender, string? filter, int pageNumber = 1, int pageSize = 10)
    //{
    //    var result = await sender.Send(new GetManualNotificationsQuery(filter, pageNumber, pageSize));

    //    if (result == null || result.Data == null)
    //    {
    //        return TypedResults.NotFound(Result<object>.Failure(
    //            StatusCodes.Status404NotFound,
    //            "No manual notifications found."
    //        ));
    //    }

    //    return TypedResults.Ok(Result<PaginatedList<ManualNotificationLogDTO>>.Success(
    //        StatusCodes.Status200OK,
    //        "Manual notifications retrieved successfully.",
    //        result.Data
    //    ));
    //}


    [Authorize]
    public async Task<IResult> GetAllNotifications(ISender sender, IHttpContextAccessor httpContextAccessor, string? filter, int pageNumber = 1, int pageSize = 10)
    {
        var language = httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        if (pageNumber < 1 || pageSize < 1)
        {
            return TypedResults.BadRequest(Result<object>.Failure(
                StatusCodes.Status400BadRequest,
                AppMessages.Get("InvalidPageNumberPageSize", language)
            ));
        }

        var result = await sender.Send(new GetAllNotificationsQuery
        {
            PageNumber = pageNumber,
            Filter = filter,
            PageSize = pageSize
        });

        if (result == null)
        {
            return TypedResults.NotFound(Result<object>.Failure(
                StatusCodes.Status404NotFound,
                AppMessages.Get("NoNotificationsFound", language)
            ));
        }

        return TypedResults.Ok(Result<PaginatedList<NotificationDTO>>.Success(
            StatusCodes.Status200OK,
            AppMessages.Get("NotificationsRetrievedSuccessfully", language),
            result.Data
        ));
    }

    [Authorize]
    public async Task<IResult> GetNotificationById(ISender sender, IHttpContextAccessor httpContextAccessor, int id)
    {
        var language = httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        if (id <= 0)
        {
            return TypedResults.BadRequest(Result<object>.Failure(
                StatusCodes.Status400BadRequest,
                AppMessages.Get("InvalidNotificationId", language)
            ));
        }

        var result = await sender.Send(new GetNotificationByIdQuery { Id = id });

        if (result == null || result.Data == null)
        {
            return TypedResults.NotFound(Result<object>.Failure(
                StatusCodes.Status404NotFound,
                AppMessages.Get("NotificationNotFound", language)
            ));
        }

        return TypedResults.Ok(Result<NotificationDTO>.Success(
            StatusCodes.Status200OK,
            AppMessages.Get("NotificationRetrievedSuccessfully", language),
            result.Data
        ));
    }

    [Authorize]
    public async Task<IResult> CreateNotification(ISender sender, IHttpContextAccessor httpContextAccessor, CreateNotificationCommand command)
    {
        var language = httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        try
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
        catch (Exception ex)
        {
            Console.WriteLine($"CreateNotification Exception: {ex.Message} | StackTrace: {ex.StackTrace}");
            return TypedResults.Json(
                Result<object>.Failure(StatusCodes.Status500InternalServerError, AppMessages.Get("UnexpectedError", language)),
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    [Authorize(Roles = nameof(Roles.Admin))]
    public async Task<IResult> CreateManualNotification(ISender sender, IHttpContextAccessor httpContextAccessor, [FromBody] CreateManualNotificationCommand command)
    {
        var language = httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        try
        {
            var result = await sender.Send(command);

            if (result.Status == StatusCodes.Status400BadRequest)
                return TypedResults.BadRequest(result);

            return TypedResults.Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CreateManualNotification Exception: {ex.Message}");
            return TypedResults.Json(
                Result<object>.Failure(StatusCodes.Status500InternalServerError, AppMessages.Get("UnexpectedError", language)),
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    [Authorize]
    public async Task<IResult> GetAllManualNotifications(ISender sender, IHttpContextAccessor httpContextAccessor, string? filter, int pageNumber = 1, int pageSize = 10)
    {
        var language = httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        var result = await sender.Send(new GetManualNotificationsQuery(filter, pageNumber, pageSize));

        if (result == null || result.Data == null)
        {
            return TypedResults.NotFound(Result<object>.Failure(
                StatusCodes.Status404NotFound,
                AppMessages.Get("NoManualNotificationsFound", language)
            ));
        }

        return TypedResults.Ok(Result<PaginatedList<ManualNotificationLogDTO>>.Success(
            StatusCodes.Status200OK,
            AppMessages.Get("ManualNotificationsRetrievedSuccessfully", language),
            result.Data
        ));
    }




    /// <summary>
    /// Updates an existing notification.
    /// </summary>
    [Authorize]
    public async Task<IResult> UpdateNotification(ISender sender, UpdateNotificationCommand command)
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

    /// <summary>
    /// Deletes a notification by ID.
    /// </summary>
    [Authorize]
    public async Task<IResult> DeleteNotification(ISender sender, int? id)
    {
        var result = await sender.Send(new DeleteNotificationCommand { Id = id });

        return result.Status switch
        {
            StatusCodes.Status404NotFound => TypedResults.NotFound(result),
            StatusCodes.Status400BadRequest => TypedResults.BadRequest(result),
            StatusCodes.Status200OK => TypedResults.Ok(result),
            _ => TypedResults.StatusCode(result.Status)
        };
    }

    [Authorize]
    public async Task<IResult> MarkNotificationReadStatus(ISender sender, [FromBody] MarkNotificationAsReadCommand command)
    {
        // Remove this check since NotificationId can be null for "mark all"
        var result = await sender.Send(command);
        return TypedResults.Ok(result);
    }


}

