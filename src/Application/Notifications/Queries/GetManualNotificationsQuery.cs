using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.Notifications;

namespace Escrow.Api.Application.Notifications.Queries;
public class GetManualNotificationsQuery : IRequest<Result<PaginatedList<ManualNotificationLogDTO>>>
{
    public string? Filter { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    // Constructor to initialize with the necessary values
    public GetManualNotificationsQuery(string? filter, int pageNumber, int pageSize)
    {
        Filter = filter;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}


public class GetManualNotificationsQueryHandler : IRequestHandler<GetManualNotificationsQuery, Result<PaginatedList<ManualNotificationLogDTO>>>
{
    private readonly IApplicationDbContext _context;

    public GetManualNotificationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<ManualNotificationLogDTO>>> Handle(GetManualNotificationsQuery request, CancellationToken cancellationToken)
    {
        try
        {


            var query = _context.ManualNotificationLogs.AsQueryable();

            // Apply filters if necessary
            if (!string.IsNullOrEmpty(request.Filter))
            {
                query = query.Where(x => x.Title.Contains(request.Filter) || x.Message.Contains(request.Filter));
            }

            // Get paginated data
            var totalCount = await query.CountAsync(cancellationToken);
            var logs = await query
                .OrderByDescending(x => x.Created)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // After fetching data from the database, transform the data in memory
            var logDtos = logs.Select(x => new ManualNotificationLogDTO
            {
                Id = x.Id,
                Title = x.Title,
                Type = x.Type,
                Message = x.Message,
                SentToAll = x.SentToAll,
                SentToUserIds = string.IsNullOrEmpty(x.SentTo)
                                ? new List<int>()
                                : x.SentTo.Split(',').Select(int.Parse).ToList(),  // This can now be done safely in memory
                Created = x.Created,
            }).ToList();

            //return new PaginatedList<ManualNotificationLogDTO>
            //{
            //    TotalCount = totalCount,
            //    Items = logDtos
            //};


            var paginatedList = new PaginatedList<ManualNotificationLogDTO>(logDtos, totalCount, request.PageNumber, request.PageSize);

            return Result<PaginatedList<ManualNotificationLogDTO>>.Success(200, "Manual notifications retrieved successfully.", paginatedList);
        }
        catch (Exception)
        {

            throw;
        }
    }
}
