namespace Escrow.Api.Application.BankDetails.Queries
{
    using Escrow.Api.Application.Authentication.Interfaces;
    using Escrow.Api.Application.Common.Constants;
    using Escrow.Api.Application.Common.Interfaces;
    using Escrow.Api.Application.DTOs;
    using Escrow.Api.Domain.Enums;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Localization;

    public record RequestOtpQuery : IRequest<Result<string>>
    {
        public required string CountryCode { get; set; }
        public required string MobileNumber { get; set; }
    }

    public class RequestOtpHandler : IRequestHandler<RequestOtpQuery, Result<string>>
    {
        private readonly IOtpManagerService _otpManagerService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IApplicationDbContext _context;


        public RequestOtpHandler(IOtpManagerService otpManagerService, IHttpContextAccessor httpContextAccessor, IApplicationDbContext context)
        {
            _otpManagerService = otpManagerService;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }


        public async Task<Result<string>> Handle(RequestOtpQuery request, CancellationToken cancellationToken)
        {
            var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

            // Find user by mobile number and role "User"
            var user = await _context.UserDetails
                .Where(u => u.PhoneNumber == request.CountryCode + request.MobileNumber && u.Role == nameof(Roles.User))
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
                return Result<string>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("InvalidMobileNumber", language));

            if (user.IsActive == false)
                return Result<string>.Failure(StatusCodes.Status403Forbidden, AppMessages.Get("PleaseContactAdministrator", language));

            var isValid = await _otpManagerService.RequestOtpAsync(request.CountryCode, request.MobileNumber);

            if (!isValid)
                return Result<string>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("InvalidMobileNumber", language));

            return Result<string>.Success(StatusCodes.Status200OK, AppMessages.Get("OtpSentSuccessfully", language));
        }



        //public async Task<Result<string>> Handle(RequestOtpQuery request, CancellationToken cancellationToken)
        //{
        //    // Retrieve the current language from the HTTP context
        //    var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;


        //    var isValid = await _otpManagerService.RequestOtpAsync(request.CountryCode, request.MobileNumber);

        //    if (!isValid)
        //        return Result<string>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("InvalidMobileNumber", language));
        //    return Result<string>.Success(StatusCodes.Status200OK, AppMessages.Get("OtpSentSuccessfully", language));
        //}
    }
}





































//namespace Escrow.Api.Application.BankDetails.Queries;

//using Escrow.Api.Application.Authentication.Interfaces;
//using Escrow.Api.Application.Common.Constants;
//using Escrow.Api.Application.DTOs;
//using Microsoft.AspNetCore.Http;

//public record RequestOtpQuery : IRequest<Result<string>>
//{
//    public required string CountryCode { get; set; }
//    public required string MobileNumber { get; set; }
//}
//public class RequestOtpHandler : IRequestHandler<RequestOtpQuery, Result<string>>
//{
//    private readonly IOtpManagerService _otpManagerService;

//    public RequestOtpHandler(IOtpManagerService otpManagerService)
//    {
//        _otpManagerService = otpManagerService;
//    }

//    public async Task<Result<string>> Handle(RequestOtpQuery request, CancellationToken cancellationToken)
//    {
//        var isValid = await _otpManagerService.RequestOtpAsync(request.CountryCode, request.MobileNumber);
//        if (!isValid)
//            return Result<string>.Failure(StatusCodes.Status400BadRequest, AppMessages.InvalidMobileNumber);

//        return Result<string>.Success(StatusCodes.Status200OK, AppMessages.OtpSentSuccessfully);
//    }
//}
