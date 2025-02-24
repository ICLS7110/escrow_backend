namespace Escrow.Api.Application.Handler; 

using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Application.Features.Queries;
using Escrow.Api.Application.Interfaces;
using Escrow.Api.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using static System.Net.WebRequestMethods;


public class OTPHandler : IRequestHandler<VerifyOTPQuery, Result<OtpDTO>>
{

    private readonly IMemoryCache _cache;
    private readonly IApplicationDbContext _context;

    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    public OTPHandler(IJwtService jwtService,IApplicationDbContext context, IOtpService otpService, IMemoryCache cache, IOtpValidationService validationService, IUserService userService)
    {
        _userService = userService;
        _cache = cache ?? throw new ArgumentNullException(nameof(cache)); 
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<Result<OtpDTO>> Handle(VerifyOTPQuery request, CancellationToken cancellationToken)
    {
        var phoneNumber = $"{request.CountryCode}{request.MobileNumber}";

        if (!_cache.TryGetValue(phoneNumber, out object? cachedOtp) || cachedOtp is not string storedOtp)
            return Result<OtpDTO>.Failure(StatusCodes.Status500InternalServerError, "Please try again.");

        if (storedOtp != request.OTP)
            return Result<OtpDTO>.Failure(StatusCodes.Status401Unauthorized, "OTP does not match.");

        _cache.Remove(phoneNumber);

        var user = await _context.UserDetails.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber, cancellationToken);

        if (user == null)
        {
            user = await _userService.CreateUserAsync(phoneNumber);
            if (user == null)
                return Result<OtpDTO>.Failure(StatusCodes.Status500InternalServerError, "User creation failed.");
        }

        if (string.IsNullOrEmpty(user.UserId))
            return Result<OtpDTO>.Failure(StatusCodes.Status404NotFound, "User not found.");

        var token = _jwtService.GetJWT(user.Id.ToString());

        var result = new OtpDTO
        {
            Token = token,
            UserId = user.Id,
            IsCompleted = user.IsProfileCompleted
        };

        return Result<OtpDTO>.Success(StatusCodes.Status200OK, "OTP verified successfully.", result);

    }
}
