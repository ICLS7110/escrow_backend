namespace Escrow.Api.Application.Handler;


using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Application.Features.Commands;
using Escrow.Api.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;


public class RequestOTPHandler : IRequestHandler<RequestOTPCommand, Result<string>>
{
    private readonly IOtpService _otpService;
    private readonly IOtpValidationService _validationService;
    private readonly IMemoryCache _cache;

    public RequestOTPHandler( IApplicationDbContext context, IOtpService otpService, IMemoryCache cache, IOtpValidationService validationService)
    {
    
        _otpService = otpService;
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _validationService = validationService;
    }

    public async Task<Result<string>> Handle(RequestOTPCommand request, CancellationToken cancellationToken)
    {

        var phoneNumber = $"{request.CountryCode}{request.MobileNumber}";
        var isPhoneNumberValid = await _validationService.ValidatePhoneNumberAsync(phoneNumber);

        if (!isPhoneNumberValid)
            return Result<string>.Failure(400, "Bad Request. Phone Number");

        var otp = await _otpService.GenerateOtpAsync();


        _cache.Set(phoneNumber, otp, TimeSpan.FromMinutes(5));

        await _otpService.SendOtpAsync(phoneNumber, otp);

        return Result<string>.Success(StatusCodes.Status200OK, "OTP verified successfully.");

    }
}
