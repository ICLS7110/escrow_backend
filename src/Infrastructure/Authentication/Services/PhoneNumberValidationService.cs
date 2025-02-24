using System.Threading.Tasks;
using PhoneNumbers;
using Escrow.Api.Application.Interfaces;

namespace Escrow.Api.Infrastructure.Authentication.Services;

public class PhoneNumberValidationService : IOtpValidationService
{
    public async Task<bool> ValidatePhoneNumberAsync(string phoneNumber)
    {
        var phoneNumberUtil = PhoneNumberUtil.GetInstance();
        try
        {
           
            PhoneNumbers.PhoneNumber number = phoneNumberUtil.Parse(phoneNumber, null);

            // Check if the phone number is valid
            return await Task.FromResult(phoneNumberUtil.IsValidNumber(number));
        }
        catch (NumberParseException)
        {
            // Return false if there's an exception (invalid phone number format)
            return await Task.FromResult(false);
        }
    }
}
