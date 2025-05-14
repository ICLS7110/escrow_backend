namespace Escrow.Api.Application.AdminAuth.Commands
{
    using Escrow.Api.Application.Common.Interfaces;
    using Escrow.Api.Application.Common.Constants;
    using Escrow.Api.Application.DTOs;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Localization;
    using System.Threading;
    using System.Threading.Tasks;

    public class AdminResetPasswordCommand : IRequest<Result<object>>
    {
        public string Email { get; init; } = string.Empty;
        public string OTP { get; init; } = string.Empty;
        public string NewPassword { get; init; } = string.Empty;
    }

    public class AdminResetPasswordCommandHandler : IRequestHandler<AdminResetPasswordCommand, Result<object>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminResetPasswordCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<object>> Handle(AdminResetPasswordCommand request, CancellationToken cancellationToken)
        {
            // Retrieve the current language from the HTTP context
            var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

            // Find the admin user by email
            var adminUser = await _context.UserDetails
                .FirstOrDefaultAsync(u => u.EmailAddress == request.Email, cancellationToken);

            if (adminUser == null)
            {
                return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("AdminNotFound", language));
            }

            // Check OTP validity
            if (string.IsNullOrEmpty(adminUser.OTP) || adminUser.OTP != request.OTP)
            {
                return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("OtpInvalidOrExpired", language));
            }

            // Hash and set the new password
            adminUser.Password = _passwordHasher.HashPassword(request.NewPassword);

            // Optional: Invalidate OTP after use
            adminUser.OTP = null;

            // Save changes to the database
            await _context.SaveChangesAsync(cancellationToken);

            return Result<object>.Success(StatusCodes.Status200OK, AppMessages.Get("PasswordResetSuccessfully", language), new { });
        }
    }
}










































//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using MediatR;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Domain.Entities.AdminPanel;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.AspNetCore.Http;
//using Escrow.Api.Application.DTOs;
//using Escrow.Api.Application.Common.Constants;

//namespace Escrow.Api.Application.AdminAuth.Commands
//{
//    public class AdminResetPasswordCommand : IRequest<Result<object>>
//    {
//        public string Email { get; init; } = string.Empty;
//        public string OTP { get; init; } = string.Empty;
//        public string NewPassword { get; init; } = string.Empty;
//    }

//    public class AdminResetPasswordCommandHandler : IRequestHandler<AdminResetPasswordCommand, Result<object>>
//    {
//        private readonly IApplicationDbContext _context;
//        private readonly IPasswordHasher _passwordHasher;

//        public AdminResetPasswordCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
//        {
//            _context = context;
//            _passwordHasher = passwordHasher;
//        }

//        public async Task<Result<object>> Handle(AdminResetPasswordCommand request, CancellationToken cancellationToken)
//        {
//            var adminUser = await _context.UserDetails
//                .FirstOrDefaultAsync(u => u.EmailAddress == request.Email, cancellationToken);

//            if (adminUser == null)
//            {
//                return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.AdminNotFound);
//            }

//            if (string.IsNullOrEmpty(adminUser.OTP) || adminUser.OTP != request.OTP)
//            {
//                return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.OtpInvalidOrExpired);
//            }

//            adminUser.Password = _passwordHasher.HashPassword(request.NewPassword);

//            // Optional: Invalidate OTP after use
//            adminUser.OTP = null;

//            await _context.SaveChangesAsync(cancellationToken);

//            return Result<object>.Success(StatusCodes.Status200OK, AppMessages.PasswordResetSuccessfully, new { });
//        }
//    }
//}
