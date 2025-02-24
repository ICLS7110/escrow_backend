namespace Escrow.Api.Application.Features.Commands;

using Escrow.Api.Application.DTOs;

public record RequestOTPCommand : IRequest<Result<string>>
{
    public required string CountryCode { get; set; }
    public required string MobileNumber { get; set; }
    public required string OTP { get; set; }
}
