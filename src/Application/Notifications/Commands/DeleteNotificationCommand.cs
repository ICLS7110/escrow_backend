using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.Notifications.Commands;
public class DeleteNotificationCommand : IRequest<Result<bool>>
{
    public int Id { get; set; }
}
public class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public DeleteNotificationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        // Validate input
        if (request.Id <= 0)
        {
            return Result<bool>.Failure(StatusCodes.Status400BadRequest, "Invalid notification ID. ID must be greater than zero.");
        }

        var notification = await _context.Notifications.FindAsync(new object[] { request.Id }, cancellationToken);

        if (notification == null)
        {
            return Result<bool>.Failure(StatusCodes.Status404NotFound, "Notification not found.");
        }

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(StatusCodes.Status200OK, "Notification deleted successfully.", true);
    }
}
