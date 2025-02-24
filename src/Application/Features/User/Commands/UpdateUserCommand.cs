namespace Escrow.Api.Application.Features.Commands;

using Escrow.Api.Application.DTOs;

public record UpdateUserCommand : IRequest<Result<UserDto>>
{
    public string? FullName { get; set; }
    public string? EmailAddress { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    // Business Fields
    public string? BusinessManagerName { get; set; }
    public string? BusinessEmail { get; set; }
    public string? VatId { get; set; }
    public string? BusinessProof { get; set; }
    public string? CompanyEmail { get; set; }
    public string? ProfilePicture { get; set; }

}
