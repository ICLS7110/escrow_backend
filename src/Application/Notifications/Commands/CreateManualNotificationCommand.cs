using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.Notifications;
using Escrow.Api.Domain.Enums;

namespace Escrow.Api.Application.Notifications.Commands;
public class CreateManualNotificationCommand : IRequest<Result<string>>
{
    public string Title { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Message { get; set; } = null!;
    public bool AllUser { get; set; } // Indicates whether to send to all users
    public List<int> SelectUser { get; set; } = new(); // List of specific user IDs

    // Validation logic: You can implement custom validation logic here
    public bool IsValid()
    {
        // Title, Type, and Message must be non-empty
        if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Type) || string.IsNullOrWhiteSpace(Message))
        {
            return false;
        }

        // If `AllUser` is false, `SelectUser` must contain at least one user ID
        if (!AllUser && SelectUser.Count == 0)
        {
            return false;
        }

        return true;
    }
}
public class CreateManualNotificationCommandHandler : IRequestHandler<CreateManualNotificationCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly INotificationService _notificationService;

    public CreateManualNotificationCommandHandler(
        IApplicationDbContext context,
        IJwtService jwtService,
        INotificationService notificationService)
    {
        _context = context;
        _jwtService = jwtService;
        _notificationService = notificationService;
    }

    public async Task<Result<string>> Handle(CreateManualNotificationCommand request, CancellationToken cancellationToken)
    {
        var createrId = _jwtService.GetUserId();

        // Validation: Ensure that the request is valid
        if (!request.IsValid())
        {
            return Result<string>.Failure(400, "Invalid request data.");
        }

        // Create the Manual Notification Log entry
        var manualNotificationLog = new ManualNotificationLog
        {
            Title = request.Title,
            Type = request.Type,
            Message = request.Message,
            SentToAll = request.AllUser,
            SentTo = request.AllUser ? null : string.Join(",", request.SelectUser), // Store user IDs as CSV string
            CreatedBy = createrId,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow,
            RecordState = RecordState.Active // Active
        };

        // Add the log entry to the database
        await _context.ManualNotificationLogs.AddAsync(manualNotificationLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Send notifications based on the 'AllUser' flag
        if (request.AllUser)
        {
            // Get all users with role "User"
            var users = await _context.UserDetails
                .Where(u => u.Role == nameof(Roles.User) && u.DeviceToken != null)
                .ToListAsync(cancellationToken);

            foreach (var user in users)
            {
                if (!string.IsNullOrEmpty(user.DeviceToken) && user.IsNotified == true)
                {
                    await _notificationService.SendPushNotificationAsync(
                        user.DeviceToken,
                        request.Title,
                        request.Message,
                        new { Type = request.Type }
                    );
                }
            }
        }
        else
        {
            // Send notification to specific selected users
            foreach (var userId in request.SelectUser)
            {
                var user = await _context.UserDetails
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                if (user != null && !string.IsNullOrEmpty(user.DeviceToken) && user.IsNotified == true)
                {
                    await _notificationService.SendPushNotificationAsync(
                        user.DeviceToken,
                        request.Title,
                        request.Message,
                        new { Type = request.Type }
                    );
                }
            }
        }

        // Return a success result
        return Result<string>.Success(200, "Manual notification created and sent successfully.");
    }
}


//public class CreateManualNotificationCommandHandler : IRequestHandler<CreateManualNotificationCommand, Result<string>>
//{
//    private readonly IApplicationDbContext _context;
//    private readonly IJwtService _jwtService;

//    public CreateManualNotificationCommandHandler(IApplicationDbContext context, IJwtService jwtService)
//    {
//        _context = context;
//        _jwtService = jwtService;
//    }

//    public async Task<Result<string>> Handle(CreateManualNotificationCommand request, CancellationToken cancellationToken)
//    {
//        var userId = _jwtService.GetUserId();
//        // Validation: Ensure that the request is valid
//        if (!request.IsValid())
//        {
//            return Result<string>.Failure(400, "Invalid request data.");
//        }

//        // Create the Manual Notification Log entry
//        var manualNotificationLog = new ManualNotificationLog
//        {
//            Title = request.Title,
//            Type = request.Type,
//            Message = request.Message,
//            SentToAll = request.AllUser,
//            SentTo = request.AllUser ? null : string.Join(",", request.SelectUser), // Store user IDs as CSV string
//            CreatedBy = userId,
//            Created = DateTimeOffset.UtcNow,
//            LastModified = DateTimeOffset.UtcNow,
//            RecordState = RecordState.Active // Active
//        };

//        // Add the log entry to the database
//        await _context.ManualNotificationLogs.AddAsync(manualNotificationLog, cancellationToken);
//        await _context.SaveChangesAsync(cancellationToken);

//        //// Send notifications to selected users if `AllUser` is false
//        //if (!request.AllUser)
//        //{
//        //    foreach (var userId in request.SelectUser)
//        //    {
//        //        var notification = new Notification
//        //        {
//        //            FromID = request.AdminId,
//        //            ToID = userId,
//        //            Type = request.Type,
//        //            Title = request.Title,
//        //            Description = request.Message,
//        //            Created = DateTimeOffset.UtcNow,
//        //            CreatedBy = request.AdminId.ToString(),
//        //            LastModified = DateTimeOffset.UtcNow,
//        //            LastModifiedBy = request.AdminId.ToString(),
//        //            IsRead = false,
//        //            RecordState = RecordState.Active
//        //        };

//        //        await _context.Notifications.AddAsync(notification, cancellationToken);
//        //    }
//        //}

//        // Save changes for notifications if any
//        await _context.SaveChangesAsync(cancellationToken);

//        // Return a success result
//        return Result<string>.Success(200, "Manual notification created and sent successfully.");
//    }
//}
