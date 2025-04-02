using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models.AML;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Escrow.Api.Application.AML.Queries
{
    public class GetAMLNotificationsQuery : IRequest<Result<List<AMLNotificationDto>>>
    {
    }

    public class GetAMLNotificationsQueryHandler : IRequestHandler<GetAMLNotificationsQuery, Result<List<AMLNotificationDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetAMLNotificationsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<AMLNotificationDto>>> Handle(GetAMLNotificationsQuery request, CancellationToken cancellationToken)
        {
            var notifications = await _context.AMLNotifications
                .AsNoTracking() // Optimized read queries
                .OrderByDescending(n => n.CreatedAt) // Fetch latest first
                .Select(n => new AMLNotificationDto
                {
                    Id = n.Id,
                    TransactionId = n.TransactionId ?? "N/A",
                    UserId = n.UserId ?? "Unknown",
                    Message = string.IsNullOrWhiteSpace(n.Message) ? "No message available" : n.Message,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync(cancellationToken);

            if (!notifications.Any())
            {
                return Result<List<AMLNotificationDto>>.Failure(StatusCodes.Status404NotFound, "No AML notifications found.");
            }

            return Result<List<AMLNotificationDto>>.Success(StatusCodes.Status200OK, "AML notifications retrieved successfully.", notifications);
        }
    }
}
