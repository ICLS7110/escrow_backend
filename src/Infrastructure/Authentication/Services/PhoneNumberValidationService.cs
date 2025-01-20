using PhoneNumbers;
using Escrow.Api.Application.Authentication.Interfaces;
using Twilio.Types;

namespace Escrow.Api.Infrastructure.Authentication.Services;

public class PhoneNumberValidationService : IOtpValidationService
{
    public bool ValidatePhoneNumber(string phoneNumber)
    {
        var phoneNumberUtil = PhoneNumberUtil.GetInstance();
        try
        {
            // Parse the phone number with the region code (null will auto-detect it)
            PhoneNumbers.PhoneNumber number = phoneNumberUtil.Parse(phoneNumber, null);

            // Check if the phone number is valid
            return phoneNumberUtil.IsValidNumber(number);
        }
        catch (NumberParseException)
        {
            // Return false if there's an exception (invalid phone number format)
            return false;
        }
    }
}

