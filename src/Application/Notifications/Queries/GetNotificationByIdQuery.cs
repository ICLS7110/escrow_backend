using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Constants;
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
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetNotificationByIdQueryHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<NotificationDTO>> Handle(GetNotificationByIdQuery request, CancellationToken cancellationToken)
    {
        var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

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
                IsRead = n.IsRead,
                Description = n.Description,
                GroupId = n.GroupId,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (notification == null)
        {
            var notFoundMessage = AppMessages.Get("NotificationNotFound", language);
            return Result<NotificationDTO>.Failure(StatusCodes.Status404NotFound, notFoundMessage);
        }

        var successMessage = AppMessages.Get("NotificationRetrievedSuccessfully", language);
        return Result<NotificationDTO>.Success(StatusCodes.Status200OK, successMessage, notification);
    }
}





























//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.Common.Models;
//using Escrow.Api.Application.DTOs;
//using Microsoft.AspNetCore.Http;

//namespace Escrow.Api.Application.Notifications.Queries;
//public class GetNotificationByIdQuery : IRequest<Result<NotificationDTO>>
//{
//    public int Id { get; set; }
//}
//public class GetNotificationByIdQueryHandler : IRequestHandler<GetNotificationByIdQuery, Result<NotificationDTO>>
//{
//    private readonly IApplicationDbContext _context;

//    public GetNotificationByIdQueryHandler(IApplicationDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<Result<NotificationDTO>> Handle(GetNotificationByIdQuery request, CancellationToken cancellationToken)
//    {
//        var notification = await _context.Notifications
//            .Where(n => n.Id == request.Id)
//            .Select(n => new NotificationDTO
//            {
//                Id = n.Id,
//                FromID = n.FromID,
//                ToID = n.ToID,
//                ContractId = n.ContractId,
//                Type = n.Type,
//                Title = n.Title,
//                IsRead = n.IsRead,
//                Description = n.Description,
//                GroupId = n.GroupId,
//            })
//            .FirstOrDefaultAsync(cancellationToken);

//        if (notification == null)
//        {
//            return Result<NotificationDTO>.Failure(StatusCodes.Status404NotFound, "Notification not found.");
//        }

//        return Result<NotificationDTO>.Success(StatusCodes.Status200OK, "Notification retrieved successfully.", notification);
//    }
//}
