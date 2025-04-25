using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.Notifications.Commands;

public class MarkNotificationAsReadCommand : IRequest<Result<NotificationReadStatusResultDto>>
{
    public int? NotificationId { get; set; }
    public bool? IsRead { get; set; }
}
public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, Result<NotificationReadStatusResultDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public MarkNotificationAsReadCommandHandler(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }


    public async Task<Result<NotificationReadStatusResultDto>> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var userId = _jwtService.GetUserId().ToInt();

        // If specific notification is provided
        if (request.NotificationId.HasValue && request.NotificationId.Value > 0)
        {
            var xnotification = await _context.Notifications
                .Where(n =>
                    n.Id == request.NotificationId.Value &&
                    n.RecordState == (int)RecordState.Active &&
                    (n.ToID == userId || n.FromID == userId || n.CreatedBy == userId.ToString()))
                .FirstOrDefaultAsync(cancellationToken);

            if (xnotification == null)
            {
                return Result<NotificationReadStatusResultDto>.Success(
                    StatusCodes.Status200OK,
                    "Notification not found.",
                    new());
            }

            xnotification.IsRead = request.IsRead;
            _context.Notifications.Update(xnotification);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<NotificationReadStatusResultDto>.Success(
                StatusCodes.Status200OK,
                request.IsRead == true ? "Notification marked as read successfully.": "Notification marked as unread successfully.",
                new NotificationReadStatusResultDto
                {
                    NotificationId = xnotification.Id,
                    IsRead = xnotification.IsRead ?? false
                });
        }

        // If no NotificationId provided, mark all related as read/unread
        var notifications = await _context.Notifications
            .Where(n =>
                n.RecordState == (int)RecordState.Active &&
                (n.ToID == userId || n.FromID == userId || n.CreatedBy == userId.ToString()))
            .ToListAsync(cancellationToken);

        if (!notifications.Any())
        {
            return Result<NotificationReadStatusResultDto>.Success(
                StatusCodes.Status200OK,
                "No notifications found to update.",
                new());
        }

        notifications.ForEach(n => n.IsRead = request.IsRead);
        _context.Notifications.UpdateRange(notifications);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<NotificationReadStatusResultDto>.Success(
            StatusCodes.Status200OK,
            request.IsRead == true ? "All notifications marked as read successfully." : "All notifications marked as unread successfully.",
            new NotificationReadStatusResultDto
            {
                NotificationId = 0,
                IsRead = request.IsRead ?? false
            });
    }


    //public async Task<Result<NotificationReadStatusResultDto>> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    //{
    //    var userId = _jwtService.GetUserId().ToInt();
    //    string xsss = RecordState.Active.ToString();
    //    if (request.NotificationId.HasValue && request.NotificationId.Value > 0)
    //    {
    //        var notification = await _context.Notifications.ToListAsync();

    //        var xnotification = notification.FirstOrDefault(n => n.Id == request.NotificationId && n.RecordState == (int)RecordState.Active);

    //        if (userId != 0)
    //        {
    //            notification.Where(n => n.ToID == userId || n.FromID == userId).FirstOrDefault();
    //        }

    //        if (notification == null)
    //        {
    //            return Result<NotificationReadStatusResultDto>.Success(
    //                StatusCodes.Status200OK,
    //                "Notification not found.", new());
    //        }

    //        //xnotification.IsRead = request.IsRead;
    //        await _context.SaveChangesAsync(cancellationToken);

    //        return Result<NotificationReadStatusResultDto>.Success(
    //            StatusCodes.Status200OK,
    //            "Notification marked successfully.",
    //            new NotificationReadStatusResultDto
    //            {
    //                //NotificationId = notification.Id,
    //                //IsRead = notification.IsRead ?? false
    //            });
    //    }

    //    // Mark all notifications for the current user
    //    var notifications = await _context.Notifications
    //        .Where(n => n.ToID == userId || n.FromID == userId || n.CreatedBy == userId.ToString())
    //        .ToListAsync(cancellationToken);

    //    if (notifications == null || notifications.Count == 0)
    //    {
    //        return Result<NotificationReadStatusResultDto>.Success(StatusCodes.Status200OK, "No notifications found to update.", new());
    //    }

    //    notifications.ForEach(n => n.IsRead = request.IsRead);
    //    _context.Notifications.UpdateRange(notifications);
    //    await _context.SaveChangesAsync(cancellationToken);

    //    return Result<NotificationReadStatusResultDto>.Success(
    //        StatusCodes.Status200OK,
    //        "All notifications marked successfully.",
    //        new NotificationReadStatusResultDto { NotificationId = 0, IsRead = request.IsRead ?? false }
    //    );
    //}
}
//public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, NotificationReadStatusResultDto>
//{
//    private readonly IApplicationDbContext _context;
//    private readonly IJwtService _jwtService;

//    public MarkNotificationAsReadCommandHandler(IApplicationDbContext context, IJwtService jwtService)
//    {
//        _context = context;
//        _jwtService = jwtService;
//    }

//    public async Task<NotificationReadStatusResultDto> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
//    {
//        var userId = _jwtService.GetUserId().ToInt();

//        if (request.NotificationId != 0)
//        {
//            var notification = await _context.Notifications
//                .FirstOrDefaultAsync(n => n.Id == request.NotificationId && (n.ToID == userId || n.FromID == userId), cancellationToken);

//            if (notification == null)
//            {
//                return new NotificationReadStatusResultDto { NotificationId = request.NotificationId, IsRead = request.IsRead ?? false };
//            }

//            notification.IsRead = request.IsRead;
//            await _context.SaveChangesAsync(cancellationToken);

//            return new NotificationReadStatusResultDto { NotificationId = notification.Id, IsRead = notification.IsRead ?? false };
//        }

//        var notifications = await _context.Notifications
//            .Where(n => n.ToID == userId || n.FromID == userId || n.CreatedBy == userId.ToString())
//            .ToListAsync(cancellationToken);

//        notifications.ForEach(n => n.IsRead = request.IsRead);

//        _context.Notifications.UpdateRange(notifications);
//        await _context.SaveChangesAsync(cancellationToken);

//        return new NotificationReadStatusResultDto { NotificationId = 0, IsRead = request.IsRead ?? false };
//    }

//    //public async Task<NotificationReadStatusResultDto> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
//    //{
//    //    var notification = await _context.Notifications.FindAsync(new object[] { request.NotificationId }, cancellationToken);

//    //    if (notification == null)
//    //    {
//    //        return new NotificationReadStatusResultDto
//    //        {
//    //            NotificationId = request.NotificationId,
//    //            IsRead = false
//    //        };
//    //    }

//    //    notification.IsRead = request.IsRead;
//    //    _context.Notifications.Update(notification);
//    //    await _context.SaveChangesAsync(cancellationToken);

//    //    return new NotificationReadStatusResultDto
//    //    {
//    //        NotificationId = notification.Id,
//    //        IsRead = notification.IsRead ?? false
//    //    };
//    //}
//}

