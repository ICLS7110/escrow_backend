using System;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Newtonsoft.Json.Linq;
using System.Linq;

public class GoogleJwtValidator
{
    // Replace with your Google Client ID
    private const string GoogleClientId = "44473283156-7op213480o9p3jagklfremnp8qunugm3.apps.googleusercontent.com";

    public static bool ValidateGoogleToken(string token)
    {
        try
        {
            // Step 1: Decode the JWT token
            var parts = token.Split('.');
            if (parts.Length != 3)
            {
                throw new ArgumentException("Invalid JWT token.");
            }

            // Decode payload (base64Url -> base64 -> json)
            var payloadJson = DecodeBase64Url(parts[1]);
            var payload = JObject.Parse(payloadJson);

            // Step 2: Validate claims
            if (!ValidateClaims(payload))
            {
                return false;
            }

            // Step 3: Verify the signature (optional step, requires fetching Google's public keys)
            if (!VerifySignature(token))
            {
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            // Log the error for debugging
            Console.WriteLine($"Error validating JWT: {ex.Message}");
            return false;
        }
    }

    // Decodes a Base64Url-encoded string
    private static string DecodeBase64Url(string base64Url)
    {
        base64Url = base64Url.Replace('-', '+').Replace('_', '/'); // URL-safe Base64 to Base64
        byte[] byteArray = Convert.FromBase64String(base64Url + new string('=', (4 - base64Url.Length % 4) % 4)); // Padding
        return System.Text.Encoding.UTF8.GetString(byteArray);
    }

    // Validates the claims of the JWT
    private static bool ValidateClaims(JObject payload)
    {
        // Validate `iss` (Issuer)
        if (payload["iss"]?.ToString() != "https://accounts.google.com" && payload["iss"]?.ToString() != "accounts.google.com")
        {
            return false;
        }

        // Validate `aud` (Audience)
        if (payload["aud"]?.ToString() != GoogleClientId)
        {
            return false;
        }

        // Validate `exp` (Expiration)
        var exp = payload["exp"]?.ToObject<long>();
        if (exp == null || exp < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        {
            return false;
        }

        return true;
    }

    // Verify the JWT signature (requires Google's public keys)
    private static bool VerifySignature(string token)
    {
        // This step requires fetching Google's public keys and verifying the signature
        // Use a library like JWT.Net or manually fetch and validate the signature

        // For this example, this method is just a placeholder.
        // You'll need to implement actual signature verification using Google's public key set.
        return true; // Return true as a placeholder
    }
}
