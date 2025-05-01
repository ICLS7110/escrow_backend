using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Infrastructure.Helpers;

public class TokenHelper
{
    public static string? GetEmailFromToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var handler = new JwtSecurityTokenHandler();

        try
        {
            var jwtToken = handler.ReadJwtToken(token);

            // Safely get the email claim
            var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "email");

            return emailClaim?.Value;
        }
        catch (Exception ex)
        {
            // Optional: log the error
            Console.WriteLine($"Token parsing error: {ex.Message}");
            return null;
        }
    }
}

