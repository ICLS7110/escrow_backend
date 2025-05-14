
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Escrow.Api.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.AdminPanel;
using Escrow.Api.Application.Common.Constants;

namespace Escrow.Api.Application.AdminAuth.Commands
{
    public record AdminChangePasswordCommand : IRequest<Result<object>>
    {
        public string Email { get; init; } = string.Empty;
        public string OldPassword { get; init; } = string.Empty;
        public string NewPassword { get; init; } = string.Empty;
        public string ConfirmNewPassword { get; init; } = string.Empty;
    }

    public class AdminChangePasswordCommandHandler : IRequestHandler<AdminChangePasswordCommand, Result<object>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminChangePasswordCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<object>> Handle(AdminChangePasswordCommand request, CancellationToken cancellationToken)
        {
            // Retrieve the current language from the HTTP context
            var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

            if (string.IsNullOrWhiteSpace(request.OldPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("OldAndNew", language));
            }

            if (request.NewPassword != request.ConfirmNewPassword)
            {
                return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("PasswordMismatch", language));
            }

            var adminUser = await _context.UserDetails
                .FirstOrDefaultAsync(u => u.EmailAddress == request.Email, cancellationToken);

            if (adminUser == null)
            {
                return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("AdminNotFound", language));
            }

            if (string.IsNullOrEmpty(adminUser.Password) ||
                !_passwordHasher.VerifyPassword(request.OldPassword, adminUser.Password))
            {
                return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("OldPasswordIncorrect", language));
            }

            if (_passwordHasher.VerifyPassword(request.NewPassword, adminUser.Password))
            {
                return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("NewPasswordCannotTheSameasOldPassword", language));
            }

            adminUser.Password = _passwordHasher.HashPassword(request.NewPassword);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<object>.Success(StatusCodes.Status200OK, AppMessages.Get("PasswordChangedSuccessfully", language));
        }
    }
}
































//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using MediatR;
//using Escrow.Api.Application.Common.Interfaces;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.AspNetCore.Http;
//using Escrow.Api.Application.DTOs;
//using Escrow.Api.Domain.Entities.AdminPanel;
//using Escrow.Api.Application.Common.Constants;

//namespace Escrow.Api.Application.AdminAuth.Commands
//{
//    public record AdminChangePasswordCommand : IRequest<Result<object>>
//    {
//        public string Email { get; init; } = string.Empty;
//        public string OldPassword { get; init; } = string.Empty;
//        public string NewPassword { get; init; } = string.Empty;
//        public string ConfirmNewPassword { get; init; } = string.Empty;
//    }

//    public class AdminChangePasswordCommandHandler : IRequestHandler<AdminChangePasswordCommand, Result<object>>
//    {
//        private readonly IApplicationDbContext _context;
//        private readonly IPasswordHasher _passwordHasher;

//        public AdminChangePasswordCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
//        {
//            _context = context;
//            _passwordHasher = passwordHasher;
//        }

//        public async Task<Result<object>> Handle(AdminChangePasswordCommand request, CancellationToken cancellationToken)
//        {
//            if (string.IsNullOrWhiteSpace(request.OldPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
//            {
//                return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.OldAndNew);
//            }

//            if (request.NewPassword != request.ConfirmNewPassword)
//            {
//                return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.PasswordMismatch);
//            }

//            var adminUser = await _context.UserDetails
//                .FirstOrDefaultAsync(u => u.EmailAddress == request.Email, cancellationToken);

//            if (adminUser == null)
//            {
//                return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.AdminNotFound);
//            }

//            if (string.IsNullOrEmpty(adminUser.Password) ||
//                !_passwordHasher.VerifyPassword(request.OldPassword, adminUser.Password))
//            {
//                return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.OldPasswordIncorrect);
//            }

//            if (_passwordHasher.VerifyPassword(request.NewPassword, adminUser.Password))
//            {
//                return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.NewPasswordCannotTheSameasOldPassword);
//            }

//            adminUser.Password = _passwordHasher.HashPassword(request.NewPassword);
//            await _context.SaveChangesAsync(cancellationToken);

//            return Result<object>.Success(StatusCodes.Status200OK, AppMessages.PasswordChangedSuccessfully);
//        }
//    }

//}
