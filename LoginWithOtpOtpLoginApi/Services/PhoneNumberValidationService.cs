using PhoneNumbers;
using Twilio.Types;

namespace OtpLoginApi.Services
{
    public class PhoneNumberValidationService
    {
        public bool ValidatePhoneNumber(string phoneNumber)
        {
            var phoneNumberUtil = PhoneNumberUtil.GetInstance();
            try
            {
                PhoneNumbers.PhoneNumber number = phoneNumberUtil.Parse(phoneNumber, null);
                return phoneNumberUtil.IsValidNumber(number);
            }
            catch (NumberParseException)
            {
                return false;
            }
        }
    }
}
