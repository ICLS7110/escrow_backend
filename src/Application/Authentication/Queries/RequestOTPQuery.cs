namespace Escrow.Api.Application.BankDetails.Queries;

using Escrow.Api.Application.Authentication.Interfaces;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;

public record RequestOtpQuery : IRequest<Result<string>>
{
    public required string CountryCode { get; set; }
    public required string MobileNumber { get; set; }
}
public class RequestOtpHandler : IRequestHandler<RequestOtpQuery, Result<string>>
{
    private readonly IOtpManagerService _otpManagerService;

    public RequestOtpHandler(IOtpManagerService otpManagerService)
    {
        _otpManagerService = otpManagerService;
    }

    public async Task<Result<string>> Handle(RequestOtpQuery request, CancellationToken cancellationToken)
    {
        var isValid = await _otpManagerService.RequestOtpAsync(request.CountryCode, request.MobileNumber);
        if (!isValid)
            return Result<string>.Failure(StatusCodes.Status400BadRequest, "Mobile Number Not Valid.");

        return Result<string>.Success(StatusCodes.Status200OK, "OTP sent successfully.");
    }
}
