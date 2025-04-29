using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.Notifications.Commands;
public class UpdateNotificationCommand : IRequest<Result<bool>>
{
    public int Id { get; set; }
    public int FromID { get; set; }
    public int ToID { get; set; }
    public int ContractId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
public class UpdateNotificationCommandHandler : IRequestHandler<UpdateNotificationCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public UpdateNotificationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(UpdateNotificationCommand request, CancellationToken cancellationToken)
    {
        // Validate input
        if (request.Id <= 0 || request.FromID <= 0 || request.ToID <= 0 || request.ContractId <= 0)
        {
            return Result<bool>.Failure(StatusCodes.Status400BadRequest, "Invalid input. IDs must be greater than zero.");
        }

        var notification = await _context.Notifications.FindAsync(new object[] { request.Id }, cancellationToken);

        if (notification == null)
        {
            return Result<bool>.Failure(StatusCodes.Status404NotFound, "Notification not found.");
        }

        // Update fields
        notification.FromID = request.FromID;
        notification.ToID = request.ToID;
        notification.ContractId = request.ContractId;
        notification.Type = request.Type;
        notification.Title = request.Title;
        notification.Description = request.Description;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(StatusCodes.Status200OK, "Notification updated successfully.", true);
    }
}
