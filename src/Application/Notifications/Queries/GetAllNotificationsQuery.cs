using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.Notifications.Queries;
public class GetAllNotificationsQuery : IRequest<Result<PaginatedList<NotificationDTO>>>
{
    public string? Filter { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
public class GetAllNotificationsQueryHandler : IRequestHandler<GetAllNotificationsQuery, Result<PaginatedList<NotificationDTO>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public GetAllNotificationsQueryHandler(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<Result<PaginatedList<NotificationDTO>>> Handle(GetAllNotificationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _jwtService.GetUserId().ToInt();

        // Get filtered and paginated notifications
        var query = _context.Notifications
            .Where(x => x.RecordState == 0 && x.ToID == userId);

        if (!string.IsNullOrEmpty(request.Filter))
        {
            query = query.Where(x => x.Title.Contains(request.Filter) || x.Description.Contains(request.Filter));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Count notifications where IsRead is false (nullable boolean handling)
        var unreadCount = await query.CountAsync(x => x.IsRead.HasValue && !x.IsRead.Value, cancellationToken);

        var notifications = new List<NotificationDTO>();
        try
        {

            notifications = await query
    .OrderByDescending(n => n.Id)
    .Skip((request.PageNumber - 1) * request.PageSize)
    .Take(request.PageSize)
    .Join(_context.ContractDetails,
        notification => notification.ContractId,
        contract => contract.Id,
        (notification, contract) => new { notification, contract })
    .Join(_context.UserDetails,
        combined => combined.notification.ToID,
        user => user.Id,
        (combined, receiver) => new { combined.notification, combined.contract, receiver })
    .Select(x => new NotificationDTO
    {
        Id = x.notification.Id,
        FromID = x.notification.FromID,
        ToID = x.notification.ToID,
        ContractId = x.notification.ContractId,
        Type = x.notification.Type,
        Title = x.notification.Title,
        Description = x.notification.Description,
        IsRead = x.notification.IsRead,
        GroupId = x.notification.GroupId,
        CreatedAt = x.notification.Created.DateTime,
        Role = x.contract.Role,
        ContractTitle = x.contract.ContractTitle,
        unreadCount = unreadCount.ToString(),
        ReceiverName = x.receiver.FullName ?? string.Empty,
        ReceiverImage = x.receiver.ProfilePicture ?? string.Empty
    })
    .ToListAsync(cancellationToken);

            // Join with Contracts to determine user role
            //notifications = await query
            //    .OrderByDescending(n => n.Id)
            //    .Skip((request.PageNumber - 1) * request.PageSize)
            //    .Take(request.PageSize)
            //    .Join(_context.ContractDetails,
            //        notification => notification.ContractId,
            //        contract => contract.Id,
            //        (notification, contract) => new { notification, contract })
            //    .Select(x => new NotificationDTO
            //    {
            //        Id = x.notification.Id,
            //        FromID = x.notification.FromID,
            //        ToID = x.notification.ToID,
            //        ContractId = x.notification.ContractId,
            //        Type = x.notification.Type,
            //        Title = x.notification.Title,
            //        Description = x.notification.Description,
            //        IsRead = x.notification.IsRead,
            //        GroupId = x.notification.GroupId,
            //        CreatedAt = x.notification.Created.DateTime,
            //        Role = x.contract.Role,
            //        ContractTitle = x.contract.ContractTitle,
            //        unreadCount = unreadCount.ToString()
            //    })
            //    .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            var exp = ex;
        }

        // Return paginated list with total count and unread count
        var paginatedList = new PaginatedList<NotificationDTO>(notifications, totalCount, request.PageNumber, request.PageSize);

        // You can include unreadCount in the result's metadata if needed
        return Result<PaginatedList<NotificationDTO>>.Success(StatusCodes.Status200OK,
            $"Notifications retrieved successfully. Unread notifications count: {unreadCount}", paginatedList);
    }
}


