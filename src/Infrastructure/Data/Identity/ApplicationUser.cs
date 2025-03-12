namespace Escrow.Api.Infrastructure.Identity;


using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    [Required]
    [DataType(DataType.PhoneNumber)]
    public override string? PhoneNumber { get; set; } = string.Empty;

    public override string? UserName
    {
        get => PhoneNumber;
        set => base.UserName = value;
    }
}
