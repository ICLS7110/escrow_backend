namespace Escrow.Api.Application.BankDetails.Queries
{
    using Escrow.Api.Application.Authentication.Interfaces;
    using Escrow.Api.Application.Common.Constants;
    using Escrow.Api.Application.DTOs;
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

        public RequestOtpHandler(IOtpManagerService otpManagerService, IHttpContextAccessor httpContextAccessor)
        {
            _otpManagerService = otpManagerService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<string>> Handle(RequestOtpQuery request, CancellationToken cancellationToken)
        {
            // Retrieve the current language from the HTTP context
            var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

            var isValid = await _otpManagerService.RequestOtpAsync(request.CountryCode, request.MobileNumber);

            if (!isValid)
                return Result<string>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("InvalidMobileNumber", language));

            return Result<string>.Success(StatusCodes.Status200OK, AppMessages.Get("OtpSentSuccessfully", language));
        }
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
