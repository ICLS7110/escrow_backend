using System;
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

namespace Escrow.Api.Application.AdminAuth.Queries
{
    // Query to fetch all details of an admin by their ID
    public class GetAdminAllDetailQuery : IRequest<Result<AllAdminDetail>>
    {
        public int AdminId { get; init; } // Made non-nullable
    }

    public class GetAdminAllDetailQueryHandler : IRequestHandler<GetAdminAllDetailQuery, Result<AllAdminDetail>>
    {
        private readonly IApplicationDbContext _context;

        public GetAdminAllDetailQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<AllAdminDetail>> Handle(GetAdminAllDetailQuery request, CancellationToken cancellationToken)
        {
            if (request.AdminId <= 0)
            {
                return Result<AllAdminDetail>.Failure(StatusCodes.Status400BadRequest, "Valid Admin ID is required.");
            }

            var adminUser = await _context.UserDetails
                .FirstOrDefaultAsync(x => x.Id == request.AdminId, cancellationToken);

            if (adminUser == null)
            {
                return Result<AllAdminDetail>.Failure(StatusCodes.Status404NotFound, "Admin not found.");
            }

            var adminDetailDto = new AllAdminDetail
            {
                Id = adminUser.Id,
                Username = adminUser.FullName,
                Email = adminUser.EmailAddress,
                Role = adminUser.Role,
                Image = adminUser.ProfilePicture,
                Created = adminUser.Created,
                CreatedBy = adminUser.CreatedBy,
                LastModified = adminUser.LastModified,
                LastModifiedBy = adminUser.LastModifiedBy,
                RecordState = Convert.ToInt32(adminUser.RecordState),
                DeletedAt = adminUser.DeletedAt,
                DeletedBy = adminUser.DeletedBy
            };

            return Result<AllAdminDetail>.Success(StatusCodes.Status200OK, "Admin details retrieved successfully.", adminDetailDto);
        }
    }
}

