

namespace Escrow.Api.Application.DTOs;
public record OtpDTO
{
    public required string Token { get; set; }
    public required int UserId { get; set; }
    public bool? IsCompleted { get; set; }
}
