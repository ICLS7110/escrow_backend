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

    }

    /// <summary>
    /// Retrieves all notifications with optional pagination.
    /// </summary>
    [Authorize]
    public async Task<IResult> GetAllNotifications(ISender sender,string? filter, int pageNumber = 1, int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            return TypedResults.BadRequest(Result<object>.Failure(
                StatusCodes.Status400BadRequest,
                "Page number and page size must be greater than zero."
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
                "No notifications found."
            ));
        }

        return TypedResults.Ok(Result<PaginatedList<NotificationDTO>>.Success(
            StatusCodes.Status200OK,
            "Notifications retrieved successfully.",
            result.Data
        ));
    }


    /// <summary>
    /// Retrieves a notification by ID.
    /// </summary>
    [Authorize]
    public async Task<IResult> GetNotificationById(ISender sender, int id)
    {
        if (id <= 0)
        {
            return TypedResults.BadRequest(Result<object>.Failure(
                StatusCodes.Status400BadRequest,
                "Invalid notification ID. ID must be greater than zero."
            ));
        }

        var result = await sender.Send(new GetNotificationByIdQuery { Id = id });

        if (result == null || result.Data == null)
        {
            return TypedResults.NotFound(Result<object>.Failure(
                StatusCodes.Status404NotFound,
                "Notification not found."
            ));
        }

        return TypedResults.Ok(Result<NotificationDTO>.Success(
            StatusCodes.Status200OK,
            "Notification retrieved successfully.",
            result.Data
        ));
    }

    [Authorize]
    public async Task<IResult> CreateNotification(ISender sender, CreateNotificationCommand command)
    {
        try
        {
            if (command == null)
            {
                Console.WriteLine("CreateNotification: Received null command.");
                return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, "Invalid request payload."));
            }

            Console.WriteLine($"CreateNotification: Received request for Title={command.Title}, Buyer={command.BuyerPhoneNumber}, Seller={command.SellerPhoneNumber}");

            var result = await sender.Send(command);

            Console.WriteLine($"CreateNotification Response: Status={result.Status}, Message={result.Message}, Data={result.Data}");

            if (result.Status == StatusCodes.Status200OK && result.Data > 0)
            {
                // Return Created (201) with the new notification ID in response
                return TypedResults.Created($"/api/notifications/{result.Data}",
                    Result<int>.Success(StatusCodes.Status201Created, "Notification created successfully.", result.Data));
            }

            return result.Status switch
            {
                StatusCodes.Status400BadRequest => TypedResults.BadRequest(result),
                StatusCodes.Status404NotFound => TypedResults.NotFound(result),
                StatusCodes.Status500InternalServerError => TypedResults.Json(result, statusCode: StatusCodes.Status500InternalServerError),
                StatusCodes.Status201Created => TypedResults.Created($"/api/notifications/{result.Data}", result),
                StatusCodes.Status200OK => TypedResults.Created($"/api/notifications/{result.Data}", result),
                _ => TypedResults.Ok(result)
            };

        }
        catch (Exception ex)
        {
            Console.WriteLine($"CreateNotification Exception: {ex.Message} | StackTrace: {ex.StackTrace}");
            return TypedResults.Json(Result<object>.Failure(StatusCodes.Status500InternalServerError, "An unexpected error occurred."),
                statusCode: StatusCodes.Status500InternalServerError);
        }
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

    //[Authorize]
    //public async Task<IResult> MarkNotificationReadStatus(ISender sender, [FromBody] MarkNotificationAsReadCommand command)
    //{
    //    if (command.NotificationId <= 0)
    //    {
    //        return TypedResults.BadRequest(new NotificationReadStatusResultDto
    //        {
    //            NotificationId = command.NotificationId,
    //            IsRead = false
    //        });
    //    }

    //    var result = await sender.Send(command);

    //    if (result.IsRead == false && result.NotificationId == command.NotificationId)
    //    {
    //        // Assuming false means "not found" when unchanged
    //        return TypedResults.NotFound(result);
    //    }

    //    return TypedResults.Ok(Result<NotificationReadStatusResultDto>.Success(
    //        StatusCodes.Status200OK,
    //        "Notification Marked successfully.",
    //        result
    //    ));
    //    //return TypedResults.Ok(result);
    //}


}

