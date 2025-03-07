namespace Escrow.Api.Domain.Entities.DTOs;

public record VerifyOtpDto
{
    public required string AccessToken { get; set; }
    public required string UserId { get; set; }
}
