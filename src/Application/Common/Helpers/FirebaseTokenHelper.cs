using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Escrow.Api.Application.Common.Helpers;
public static class FirebaseTokenHelper
{
    public static async Task<Dictionary<string, string>?> ExtractClaimsAsync(string token)
    {
        var issuer = "https://securetoken.google.com/welink-sa-test";
        var audience = "welink-sa-test";
        var httpClient = new HttpClient();

        try
        {
            token = token?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(token) || token.Split('.').Length != 3)
                return null;

            string jwksUrl = "https://www.googleapis.com/service_accounts/v1/jwk/securetoken@system.gserviceaccount.com";
            string keysJson = await httpClient.GetStringAsync(jwksUrl);
            var keys = new JsonWebKeySet(keysJson);

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = keys.Keys,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            ClaimsPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken _);
            var email = principal.FindFirst("email")?.Value;

            if (string.IsNullOrEmpty(email))
            {
                var firebaseClaim = principal.FindFirst("firebase")?.Value;

                if (!string.IsNullOrEmpty(firebaseClaim))
                {
                    try
                    {
                        var firebaseObj = JsonSerializer.Deserialize<JsonElement>(firebaseClaim);
                        if (firebaseObj.TryGetProperty("identities", out var identities) &&
                            identities.TryGetProperty("email", out var emailArray) &&
                            emailArray.ValueKind == JsonValueKind.Array &&
                            emailArray.GetArrayLength() > 0)
                        {
                            email = emailArray[0].GetString();
                        }
                    }
                    catch
                    {
                        // fallback if parsing fails
                        email = "";
                    }
                }
            }

            return new Dictionary<string, string>
            {
                ["email"] = email ?? "",
                ["name"] = principal.FindFirst("name")?.Value ?? "",
                ["picture"] = principal.FindFirst("picture")?.Value ?? "",
                ["user_id"] = principal.FindFirst("user_id")?.Value ?? principal.FindFirst("sub")?.Value ?? ""
            };

        }
        catch
        {
            return null;
        }
    }
}
