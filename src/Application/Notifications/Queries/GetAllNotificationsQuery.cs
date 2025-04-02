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
public class GetAllNotificationsQuery : IRequest<Result<PaginatedList<NotificationDTO>>>
{
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
        var userId = _jwtService.GetUserId();
        var query = _context.Notifications.AsQueryable();


        if (!string.IsNullOrEmpty(userId))
        {
            query.Where(x => x.CreatedBy == userId).ToList();
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var notifications = await query
            .OrderByDescending(n => n.Id) // You can order by CreatedDate if available
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
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
            .ToListAsync(cancellationToken);

        var paginatedList = new PaginatedList<NotificationDTO>(notifications, totalCount, request.PageNumber, request.PageSize);

        return Result<PaginatedList<NotificationDTO>>.Success(StatusCodes.Status200OK, "Notifications retrieved successfully.", paginatedList);
    }
}
