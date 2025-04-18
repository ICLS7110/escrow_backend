

using Escrow.Api.Application.Common.Helpers;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Application.UserPanel.Queries.GetUsers;
using Escrow.Api.Domain.Entities.TeamMembers;
using Escrow.Api.Domain.Entities.UserPanel;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Escrow.Api.Application.TeamsManagement.Queries
{
    public class GetAllTeamsQuery : IRequest<Result<PaginatedList<TeamDTO>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? RoleType { get; set; } // 🔹 Filter by Role
        public bool? IsActive { get; set; } // 🔹 Filter by Active/Inactive
        public int? LastDays { get; set; } // 🔹 Filter by Date (7 or 30 days)
    }

    public class GetAllTeamsQueryHandler : IRequestHandler<GetAllTeamsQuery, Result<PaginatedList<TeamDTO>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IJwtService _jwtService;

        public GetAllTeamsQueryHandler(IApplicationDbContext context, IJwtService jwtService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        }

        public async Task<Result<PaginatedList<TeamDTO>>> Handle(GetAllTeamsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                int pageNumber = request.PageNumber;
                int pageSize = request.PageSize;

                var authenticatedUserId = _jwtService.GetUserId().ToInt();
                if (authenticatedUserId == 0)
                {
                    return Result<PaginatedList<TeamDTO>>.Failure(StatusCodes.Status401Unauthorized, "Unauthorized request.");
                }

                var query = from teamMember in _context.TeamMembers.AsNoTracking()
                            where teamMember.CreatedBy == authenticatedUserId.ToString()
                                && teamMember.IsDeleted == false
                            join user in _context.UserDetails.AsNoTracking()
                            on teamMember.UserId equals user.Id.ToString() into userGroup
                            from user in userGroup.DefaultIfEmpty()
                            select new TeamDTO
                            {
                                TeamId = teamMember.Id.ToString(),
                                UserId = teamMember.UserId,
                                RoleType = teamMember.RoleType,
                                ContractId = teamMember.ContractId,
                                IsActive = teamMember.IsActive,
                                Created = teamMember.Created, // ✅ Assuming Created is already DateTime
                                User = user != null ? new UserDetailDto
                                {
                                    FullName = user.FullName,
                                    EmailAddress = user.EmailAddress,
                                    PhoneNumber = PhoneNumberHelper.ExtractPhoneNumberWithoutCountryCode(user.PhoneNumber), // ✅ Call Helper
                                    CountryCode = PhoneNumberHelper.ExtractCountryCode(user.PhoneNumber), // ✅ Call Helper
                                } : null
                            };

                // ✅ Apply filters before executing the query
                if (!string.IsNullOrEmpty(request.RoleType))
                {
                    query = query.Where(t => t.RoleType == request.RoleType);
                }

                if (request.IsActive.HasValue)
                {
                    query = query.Where(t => t.IsActive == request.IsActive.Value);
                }

                // ✅ Execute query first before applying date filter in-memory
                var teamList = await query.ToListAsync(cancellationToken);

                if (request.LastDays.HasValue)
                {
                    var dateLimit = DateTime.UtcNow.AddDays(-request.LastDays.Value);
                    teamList = teamList.Where(t => t.Created != null && t.Created >= dateLimit).ToList();
                }

                var totalCount = teamList.Count;
                if (totalCount == 0)
                {
                    return Result<PaginatedList<TeamDTO>>.Failure(StatusCodes.Status404NotFound, "No matching team members found.");
                }

                var paginatedTeams = teamList
                    .OrderByDescending(t => t.TeamId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var paginatedList = new PaginatedList<TeamDTO>(paginatedTeams, totalCount, pageNumber, pageSize);

                return Result<PaginatedList<TeamDTO>>.Success(StatusCodes.Status200OK, "Teams retrieved successfully.", paginatedList);
            }
            catch (Exception ex)
            {
                return Result<PaginatedList<TeamDTO>>.Failure(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
    }
}


