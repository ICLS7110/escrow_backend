using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.Notifications.Queries;
public class GetNotificationByIdQuery : IRequest<Result<NotificationDTO>>
{
    public int Id { get; set; }
}
public class GetNotificationByIdQueryHandler : IRequestHandler<GetNotificationByIdQuery, Result<NotificationDTO>>
{
    private readonly IApplicationDbContext _context;

    public GetNotificationByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<NotificationDTO>> Handle(GetNotificationByIdQuery request, CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications
            .Where(n => n.Id == request.Id)
            .Select(n => new NotificationDTO
            {
                Id = n.Id,
                FromID = n.FromID,
                ToID = n.ToID,
                ContractId = n.ContractId,
                Type = n.Type,
                Title = n.Title,
                Description = n.Description,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (notification == null)
        {
            return Result<NotificationDTO>.Failure(StatusCodes.Status404NotFound, "Notification not found.");
        }

        return Result<NotificationDTO>.Success(StatusCodes.Status200OK, "Notification retrieved successfully.", notification);
    }
}
