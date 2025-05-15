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


}
