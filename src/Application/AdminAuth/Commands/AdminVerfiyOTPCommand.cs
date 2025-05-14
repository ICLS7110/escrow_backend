using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Escrow.Api.Application.Common.Constants;

namespace Escrow.Api.Application.AdminAuth.Commands
{
    public class AdminVerifyOTPCommand : IRequest<Result<string>>
    {
        public string Email { get; init; } = string.Empty;
        public string OTP { get; init; } = string.Empty;
    }

    public class AdminVerifyOTPCommandHandler : IRequestHandler<AdminVerifyOTPCommand, Result<string>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminVerifyOTPCommandHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<string>> Handle(AdminVerifyOTPCommand request, CancellationToken cancellationToken)
        {
            // Retrieve the current language from the HTTP context
            var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.OTP))
            {
                return Result<string>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("EmailAndOtpRequired", language));
            }

            var adminUser = await _context.UserDetails
                .FirstOrDefaultAsync(u => u.EmailAddress == request.Email, cancellationToken);

            if (adminUser == null)
            {
                return Result<string>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("AdminNotFound", language));
            }

            if (adminUser.OTP != request.OTP)
            {
                return Result<string>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("OtpInvalidOrExpired", language));
            }

            return Result<string>.Success(StatusCodes.Status200OK, AppMessages.Get("OtpVerified", language));
        }
    }
}






































//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using MediatR;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.DTOs;
//using Microsoft.AspNetCore.Http;
//using Microsoft.EntityFrameworkCore;
//using Escrow.Api.Application.Common.Constants;

//namespace Escrow.Api.Application.AdminAuth.Commands
//{
//    public class AdminVerifyOTPCommand : IRequest<Result<string>>
//    {
//        public string Email { get; init; } = string.Empty;
//        public string OTP { get; init; } = string.Empty;
//    }

//    public class AdminVerifyOTPCommandHandler : IRequestHandler<AdminVerifyOTPCommand, Result<string>>
//    {
//        private readonly IApplicationDbContext _context;

//        public AdminVerifyOTPCommandHandler(IApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<Result<string>> Handle(AdminVerifyOTPCommand request, CancellationToken cancellationToken)
//        {
//            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.OTP))
//            {
//                return Result<string>.Failure(StatusCodes.Status400BadRequest, AppMessages.EmailAndOtpRequired );
//            }

//            var adminUser = await _context.UserDetails
//                .FirstOrDefaultAsync(u => u.EmailAddress == request.Email, cancellationToken);

//            if (adminUser == null)
//            {
//                return Result<string>.Failure(StatusCodes.Status404NotFound, AppMessages.AdminNotFound);
//            }

//            if (adminUser.OTP != request.OTP)
//            {
//                return Result<string>.Failure(StatusCodes.Status400BadRequest, AppMessages.OtpInvalidOrExpired);
//            }

//            return Result<string>.Success(StatusCodes.Status200OK, AppMessages.OtpVerified);
//        }
//    }
//}
