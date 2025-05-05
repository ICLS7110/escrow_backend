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
using Escrow.Api.Domain.Enums;
using Escrow.Api.Domain.Entities.UserPanel;

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
        private readonly IJwtService _jwtService;

        private readonly IPasswordHasher _passwordHasher;

        public GetAdminDetailQueryHandler(IApplicationDbContext context,IMapper mapper,IJwtService jwtService,IPasswordHasher passwordHasher)
        {
            _context = context;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
        }

        public async Task<Result<AdminLoginDTO>> Handle(GetAdminDetailQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return Result<AdminLoginDTO>.Failure(StatusCodes.Status400BadRequest, "Email and Password are required.");
            }

            var adminUser = await _context.UserDetails
                .FirstOrDefaultAsync(x => x.EmailAddress == request.Email, cancellationToken);

            if (adminUser == null || !_passwordHasher.VerifyPassword(request.Password!, adminUser.Password!))
            {
                return Result<AdminLoginDTO>.Failure(StatusCodes.Status404NotFound, "Invalid credentials.");
            }

            var adminDto = new AdminLoginDTO
            {
                Id = adminUser.Id,
                Email = adminUser.EmailAddress,
                PasswordHash = adminUser.Password,
                Role = adminUser.Role,
                Username = adminUser.FullName,
                Token = _jwtService.GetJWT(adminUser.Id.ToString())
            };

            return Result<AdminLoginDTO>.Success(StatusCodes.Status200OK, "Admin login successfully.", adminDto);
        }

    }
}
