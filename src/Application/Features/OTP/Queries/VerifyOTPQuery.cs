namespace Escrow.Api.Application.Features.Queries;

using Escrow.Api.Application.DTOs;

public record VerifyOTPQuery : IRequest<Result<OtpDTO>>
{
    public required string CountryCode { get; set; }
    public required string MobileNumber { get; set; }
    public required string OTP { get; set; }

}
