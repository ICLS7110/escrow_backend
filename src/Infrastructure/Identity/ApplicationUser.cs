using Microsoft.AspNetCore.Identity;

namespace Escrow.Api.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; } = string.Empty; // Ensuring non-null values
    public string Role { get; set; } = "User"; // Default to "User"

    // Override UserName based on Role
    public override string? UserName
    {
        get => Role == "Admin" ? Email! : PhoneNumber!;
        set => base.UserName = value;
    }
}
