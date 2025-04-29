using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Domain.Entities.AdminPanel;
using Escrow.Api.Application.Common.Mappings;

namespace Escrow.Api.Application.AdminAuth.Queries
{
    public class GetAdminListingsQuery : IRequest<Result<PaginatedList<AllAdminDetail>>>
    {
        public string? Role { get; init; }
        public string? Name { get; init; }
        public int? PageNumber { get; init; } = 1;
        public int? PageSize { get; init; } = 10;
    }

    public class GetAdminListingsQueryHandler : IRequestHandler<GetAdminListingsQuery, Result<PaginatedList<AllAdminDetail>>>
    {
        private readonly IApplicationDbContext _context;

        public GetAdminListingsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<PaginatedList<AllAdminDetail>>> Handle(GetAdminListingsQuery request, CancellationToken cancellationToken)
        {

            int pageNumber = request.PageNumber ?? 1;
            int pageSize = request.PageSize ?? 10;


            var query = _context.UserDetails.AsQueryable();

            if (!string.IsNullOrEmpty(request.Role))
            {

                query = query.Where(x => x.Role!.ToLower() == request.Role!.ToLower());
            }

            if (!string.IsNullOrEmpty(request.Name))
            {
                query = query.Where(x => x.FullName != null && x.FullName.Contains(request.Name));
            }

            var totalRecords = await query.CountAsync(cancellationToken);

            var listings = await query
                .Where(x => x.IsDeleted == false)
                .Select(x => new AllAdminDetail
                {
                    Id = x.Id,
                    Username = x.FullName,
                    Email = x.EmailAddress,
                    PasswordHash = x.Password,
                    Role = x.Role,
                    Image = x.ProfilePicture,
                    Created = x.Created,
                    CreatedBy = x.CreatedBy,
                    LastModified = x.LastModified,
                    LastModifiedBy = x.LastModifiedBy,
                    RecordState = Convert.ToInt32(x.RecordState),
                    DeletedAt = x.DeletedAt,
                    DeletedBy = x.DeletedBy,
                    IsActive = x.IsActive,
                })
                .OrderByDescending(x => x.LastModified)
                .PaginatedListAsync(pageNumber, pageSize);

            return Result<PaginatedList<AllAdminDetail>>.Success(StatusCodes.Status200OK, "Listings fetched successfully.", listings);
        }
    }
}
