using System;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.Notifications.Commands
{
    public class DeleteNotificationCommand : IRequest<Result<object>>
    {
        public int? Id { get; set; }
    }
    public class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, Result<object>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IJwtService _jwtTokenService;

        public DeleteNotificationCommandHandler(IApplicationDbContext context, IJwtService jwtTokenService)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<Result<object>> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
        {
            var userId = _jwtTokenService.GetUserId().ToInt();
            // Case 1: Delete single notification by ID
            if (request.Id.HasValue && request.Id.Value > 0)
            {
                if (request.Id <= 0)
                {
                    return Result<object>.Failure(StatusCodes.Status400BadRequest, "Invalid notification ID. ID must be greater than zero.");
                }

                var notification = await _context.Notifications.FindAsync(new object[] { request.Id.Value }, cancellationToken);
                if (notification == null)
                {
                    return Result<object>.Success(StatusCodes.Status200OK, "Notification not found.",new());
                }
                notification.RecordState = RecordState.Deleted;
                notification.DeletedBy = userId;
                notification.DeletedAt = DateTime.UtcNow;
                //_context.Notifications.Remove(notification);
                await _context.SaveChangesAsync(cancellationToken);

                return Result<object>.Success(StatusCodes.Status200OK, "Notification deleted successfully.", new
                {
                    DeletedNotificationId = request.Id,
                    DeletedAt = DateTime.UtcNow
                });
            }

            // Case 2: Bulk delete for logged-in user
          

            var notifications = await _context.Notifications
                .Where(n => n.ToID == userId || n.CreatedBy == userId.ToString())
                .ToListAsync(cancellationToken);

            if (!notifications.Any())
            {
                return Result<object>.Success(StatusCodes.Status200OK, "No notifications found for this user.",new());
            }

            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<object>.Success(StatusCodes.Status200OK, "All notifications deleted successfully.", new
            {
                DeletedUserId = userId,
                DeletedAt = DateTime.UtcNow
            });
        }
    }

    //public class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, Result<object>>
    //{
    //    private readonly IApplicationDbContext _context;

    //    public DeleteNotificationCommandHandler(IApplicationDbContext context)
    //    {
    //        _context = context;
    //    }

    //    public async Task<Result<object>> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    //    {
    //        // Validate input
    //        if (request.Id <= 0)
    //        {
    //            return Result<object>.Failure(StatusCodes.Status400BadRequest, "Invalid notification ID. ID must be greater than zero.");
    //        }

    //        var notification = await _context.Notifications.FindAsync(new object[] { request.Id }, cancellationToken);

    //        if (notification == null)
    //        {
    //            return Result<object>.Failure(StatusCodes.Status404NotFound, "Notification not found.");
    //        }

    //        _context.Notifications.Remove(notification);
    //        await _context.SaveChangesAsync(cancellationToken);

    //        var resultObj = new
    //        {
    //            DeletedNotificationId = request.Id,
    //            DeletedAt = DateTime.UtcNow
    //        };

    //        return Result<object>.Success(StatusCodes.Status200OK, "Notification deleted successfully.", resultObj);
    //    }
    //}
}








































//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.DTOs;
//using Microsoft.AspNetCore.Http;

//namespace Escrow.Api.Application.Notifications.Commands;
//public class DeleteNotificationCommand : IRequest<Result<bool>>
//{
//    public int Id { get; set; }
//}
//public class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, Result<bool>>
//{
//    private readonly IApplicationDbContext _context;

//    public DeleteNotificationCommandHandler(IApplicationDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<Result<bool>> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
//    {
//        // Validate input
//        if (request.Id <= 0)
//        {
//            return Result<bool>.Failure(StatusCodes.Status400BadRequest, "Invalid notification ID. ID must be greater than zero.");
//        }

//        var notification = await _context.Notifications.FindAsync(new object[] { request.Id }, cancellationToken);

//        if (notification == null)
//        {
//            return Result<bool>.Failure(StatusCodes.Status404NotFound, "Notification not found.");
//        }

//        _context.Notifications.Remove(notification);
//        await _context.SaveChangesAsync(cancellationToken);

//        return Result<bool>.Success(StatusCodes.Status200OK, "Notification deleted successfully.", true);
//    }
//}
