using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Authentication.Interfaces;
using Escrow.Api.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Escrow.Api.Application.Common.Models;

namespace Escrow.Api.Application.AdminAuth.Queries
{
    public class GetAdminDetailQuery : IRequest<Result<AdminLoginDTO>>
    {
        public string? Email { get; init; }
        public string? Password { get; init; }
    }

    public class GetAdminDetailQueryHandler : IRequestHandler<GetAdminDetailQuery, Result<AdminLoginDTO>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IJwtService _jwtService;

        public GetAdminDetailQueryHandler(IApplicationDbContext context, IMapper mapper, IJwtService jwtService)
        {
            _context = context;
            _mapper = mapper;
            _jwtService = jwtService;
        }

        public async Task<Result<AdminLoginDTO>> Handle(GetAdminDetailQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return Result<AdminLoginDTO>.Failure(StatusCodes.Status400BadRequest, "Email and Password are required.");
            }

            var adminUser = await _context.AdminUsers
                .Where(x => x.Email == request.Email && x.PasswordHash == request.Password)
                .FirstOrDefaultAsync(cancellationToken);

            if (adminUser == null)
            {
                return Result<AdminLoginDTO>.Failure(StatusCodes.Status404NotFound, "Invalid credentials.");
            }

            var adminDto = new AdminLoginDTO
            {
                Email = adminUser.Email,
                PasswordHash = adminUser.PasswordHash,
                Role = adminUser.Role,
                Username = adminUser.Username,
                Token = _jwtService.GetJWT(adminUser.Id.ToString(), "Admin")
            };

            return Result<AdminLoginDTO>.Success(StatusCodes.Status200OK, "Admin login successfully.", adminDto);
        }
    }
}
