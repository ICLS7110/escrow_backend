
using Escrow.Api.Application.Authentication.Interfaces;
using Escrow.Api.Infrastructure.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Microsoft.Extensions.Options;

namespace Escrow.Api.Infrastructure.Authentication.Services;
public class TwilioOtpService
{
    private readonly TwilioSettings _twilioSettings;

    public TwilioOtpService(IOptions<TwilioSettings> twilioSettings)
    {
        _twilioSettings = twilioSettings.Value;
    }

    public string GenerateOtp()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    public void SendOtp(string phoneNumber, string otp)
    {
        TwilioClient.Init(_twilioSettings.AccountSid, _twilioSettings.AuthToken);

        var message = MessageResource.Create(
            body: $"Your OTP is {otp}",
            from: new PhoneNumber(_twilioSettings.PhoneNumber),
            to: new PhoneNumber(phoneNumber)
        );

        Console.WriteLine($"OTP sent to {phoneNumber}: {otp}");
    }
}
