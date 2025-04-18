using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Authentication.Interfaces;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Mappings;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.Authentication;
using Escrow.Api.Domain.Entities.DTOs;
using Escrow.Api.Domain.Entities.UserPanel;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.BankDetails.Queries;
public record VerifyOTPQuery : IRequest<Result<VerifyOtpDto>>
{
    public required string CountryCode { get; set; }
    public required string MobileNumber { get; set; }
    public required string Otp { get; set; }
}
public class VerifyOTPHandler : IRequestHandler<VerifyOTPQuery, Result<VerifyOtpDto>>
{
    private readonly IOtpManagerService _otpManagerService;
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    public VerifyOTPHandler(IOtpManagerService otpManagerService, IUserService userService, IJwtService jwtService)
    {
        _otpManagerService = otpManagerService;
        _userService = userService;
        _jwtService = jwtService;
    }

    public async Task<Result<VerifyOtpDto>> Handle(VerifyOTPQuery request, CancellationToken cancellationToken)
    {
        var phoneNumber = $"{request.CountryCode}{request.MobileNumber}";
        var isValid = _otpManagerService.VerifyOtpAsync(phoneNumber, request.Otp);
        if (!isValid)
            return Result<VerifyOtpDto>.Failure(StatusCodes.Status404NotFound, $"OTP Not Valid");

        var res = await _userService.FindUserAsync(phoneNumber);
        if (res.Data == null)
            res = await _userService.CreateUserAsync(phoneNumber);

        if (res.Data is null)
            return Result<VerifyOtpDto>.Failure(StatusCodes.Status404NotFound, $"Not Found");

        var token = _jwtService.GetJWT(res.Data.Id.ToString());
        var result = new VerifyOtpDto
        {
            AccessToken = token,
            UserId = res.Data.Id.ToString(),
            IsProfileCompleted=res.Data.IsProfileCompleted
        };
        return Result<VerifyOtpDto>.Success(StatusCodes.Status200OK, "OTP verified successfully.", result);
        
    }
}
