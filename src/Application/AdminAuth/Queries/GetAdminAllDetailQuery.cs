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
using Escrow.Api.Application.Common.Constants;
using Microsoft.Extensions.Localization;
using Escrow.Api.Application.Common.Helpers;

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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetAdminAllDetailQueryHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<AllAdminDetail>> Handle(GetAdminAllDetailQuery request, CancellationToken cancellationToken)
        {
            // Check if HttpContext is available and then get the language
            var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;  // Use null-coalescing operator to fall back to English

            if (request.AdminId <= 0)
            {
                return Result<AllAdminDetail>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("ValidadminId", language));
            }

            var adminUser = await _context.UserDetails
                .FirstOrDefaultAsync(x => x.Id == request.AdminId, cancellationToken);

            if (adminUser == null)
            {
                return Result<AllAdminDetail>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("AdminNotFound", language));
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

            return Result<AllAdminDetail>.Success(StatusCodes.Status200OK, AppMessages.Get("AdminDetailRetrieved", language), adminDetailDto);
        }
    }











    //public class GetAdminAllDetailQueryHandler : IRequestHandler<GetAdminAllDetailQuery, Result<AllAdminDetail>>
    //{
    //    private readonly IApplicationDbContext _context;
    //    private readonly IStringLocalizer<Messages> _localizer;

    //    public GetAdminAllDetailQueryHandler(IApplicationDbContext context, IStringLocalizer<Messages> localizer)
    //    {
    //        _context = context;
    //        _localizer = localizer;
    //    }

    //    public async Task<Result<AllAdminDetail>> Handle(GetAdminAllDetailQuery request, CancellationToken cancellationToken)
    //    {
    //        if (request.AdminId <= 0)
    //        {
    //            return Result<AllAdminDetail>.Failure(StatusCodes.Status400BadRequest, Loca.GetMessage("UserNotFound"));
    //        }

    //        var adminUser = await _context.UserDetails
    //            .FirstOrDefaultAsync(x => x.Id == request.AdminId, cancellationToken);

    //        if (adminUser == null)
    //        {
    //            return Result<AllAdminDetail>.Failure(StatusCodes.Status404NotFound, _localizer["AdminNotFound"]);
    //        }

    //        var adminDetailDto = new AllAdminDetail
    //        {
    //            Id = adminUser.Id,
    //            Username = adminUser.FullName,
    //            Email = adminUser.EmailAddress,
    //            Role = adminUser.Role,
    //            Image = adminUser.ProfilePicture,
    //            Created = adminUser.Created,
    //            CreatedBy = adminUser.CreatedBy,
    //            LastModified = adminUser.LastModified,
    //            LastModifiedBy = adminUser.LastModifiedBy,
    //            RecordState = Convert.ToInt32(adminUser.RecordState),
    //            DeletedAt = adminUser.DeletedAt,
    //            DeletedBy = adminUser.DeletedBy
    //        };

    //        return Result<AllAdminDetail>.Success(StatusCodes.Status200OK, _localizer["AdminDetailRetrieved"], adminDetailDto);
    //    }

    //}
    //public class GetAdminAllDetailQueryHandler : IRequestHandler<GetAdminAllDetailQuery, Result<AllAdminDetail>>
    //{
    //    private readonly IApplicationDbContext _context;
    //    private readonly IStringLocalizer<Messages> _localizer;

    //    public GetAdminAllDetailQueryHandler(IApplicationDbContext context,IStringLocalizer<Messages> localizer)
    //    {
    //        _context = context;
    //        _localizer = localizer;
    //    }

    //    public async Task<Result<AllAdminDetail>> Handle(GetAdminAllDetailQuery request, CancellationToken cancellationToken)
    //    {
    //        if (request.AdminId <= 0)
    //        {

    //            return Result<AllAdminDetail>.Failure(StatusCodes.Status400BadRequest, AppMessages.ValidadminId);
    //        }

    //        var adminUser = await _context.UserDetails
    //            .FirstOrDefaultAsync(x => x.Id == request.AdminId, cancellationToken);

    //        if (adminUser == null)
    //        {
    //            return Result<AllAdminDetail>.Failure(StatusCodes.Status404NotFound, AppMessages.AdminNotFound);
    //        }

    //        var adminDetailDto = new AllAdminDetail
    //        {
    //            Id = adminUser.Id,
    //            Username = adminUser.FullName,
    //            Email = adminUser.EmailAddress,
    //            Role = adminUser.Role,
    //            Image = adminUser.ProfilePicture,
    //            Created = adminUser.Created,
    //            CreatedBy = adminUser.CreatedBy,
    //            LastModified = adminUser.LastModified,
    //            LastModifiedBy = adminUser.LastModifiedBy,
    //            RecordState = Convert.ToInt32(adminUser.RecordState),
    //            DeletedAt = adminUser.DeletedAt,
    //            DeletedBy = adminUser.DeletedBy
    //        };

    //        return Result<AllAdminDetail>.Success(StatusCodes.Status200OK, AppMessages.AdminDetailRetrieved, adminDetailDto);
    //    }
    //}
}
