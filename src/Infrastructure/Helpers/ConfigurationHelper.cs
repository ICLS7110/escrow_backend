
using Microsoft.Extensions.Configuration;

namespace Escrow.Api.Infrastructure.Helpers
{
    public static class ConfigurationHelper
    {
        public static IConfiguration? _config;

        public static void InitializeConfig(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config), "Configuration cannot be null.");
        }

        public static string ConnectionString => _config?.GetConnectionString("Escrow")
            ?? throw new InvalidOperationException("ConnectionString 'Escrow' is missing in configuration.");

        public static string JwtIssuerSigningKey => _config?["Jwt:IssuerSigningKey"]
            ?? throw new InvalidOperationException("Jwt:IssuerSigningKey is missing in configuration.");

        public static string JwtValidIssuer => _config?["Jwt:ValidIssuer"]
            ?? throw new InvalidOperationException("Jwt:ValidIssuer is missing in configuration.");

        public static string JwtValidAudience => _config?["Jwt:ValidAudience"]
            ?? throw new InvalidOperationException("Jwt:ValidAudience is missing in configuration.");

        public static int AuthTokenExpiry
        {
            get
            {
                var value = _config?["AuthTokenExpiry"];
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new InvalidOperationException("AuthTokenExpiry is missing in configuration.");
                }

                if (!int.TryParse(value, out var expiry))
                {
                    throw new FormatException("AuthTokenExpiry must be a valid integer.");
                }

                return expiry;
            }
        }

    }
}



