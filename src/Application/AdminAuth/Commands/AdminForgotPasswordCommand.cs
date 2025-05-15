using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Entities.AdminPanel;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Escrow.Api.Application.DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace Escrow.Api.Application.AdminAuth.Commands
{
    public record AdminForgotPasswordCommand : IRequest<Result<object>> // Changed Result<string> to Result<object>
    {
        public string Email { get; init; } = string.Empty;
    }

    public class AdminForgotPasswordCommandHandler : IRequestHandler<AdminForgotPasswordCommand, Result<object>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IMemoryCache _cache;


        public AdminForgotPasswordCommandHandler(
            IApplicationDbContext context,
            IJwtService jwtService,
            IMemoryCache cache)
        {
            _context = context;
            _jwtService = jwtService;
            _cache = cache;
        }

        public async Task<Result<object>> Handle(AdminForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            // Check if the email exists
            var adminUser = await _context.UserDetails
                .FirstOrDefaultAsync(u => u.EmailAddress == request.Email, cancellationToken);

            if (adminUser == null)
            {
                return Result<object>.Failure(StatusCodes.Status404NotFound, "Email not found.");
            }

            string otp = "1234";//new Random().Next(100000, 999999).ToString();
            _cache.Set(request.Email, otp, TimeSpan.FromMinutes(5)); // Store OTP in cache for 5 minutes


            adminUser.OTP = otp;
            await _context.SaveChangesAsync(cancellationToken);

            return Result<object>.Success(StatusCodes.Status200OK, "OTP sent successfully.", new { });
        }
    }
}

