using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Microsoft.Extensions.Options;
using Escrow.Domain.UserPanel;

public class OtpService
{
    private readonly TwilioSettings _twilioSettings;

    public OtpService(IOptions<TwilioSettings> twilioSettings)
    {
        _twilioSettings = twilioSettings.Value;
    }

    // OTP Generation Logic
    public string GenerateOtp()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    // Send OTP via SMS using Twilio
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
