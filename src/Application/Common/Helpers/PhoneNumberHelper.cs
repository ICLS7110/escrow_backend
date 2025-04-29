using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhoneNumbers;

namespace Escrow.Api.Application.Common.Helpers;
public static class PhoneNumberHelper
{
    public static string ExtractPhoneNumberWithoutCountryCode(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return string.Empty; // Return empty string if null or whitespace

        var phoneUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();

        try
        {
            var number = phoneUtil.Parse(phoneNumber, null);
            var countryCode = number.CountryCode.ToString();
            return phoneNumber.StartsWith($"+{countryCode}") ? phoneNumber.Substring(countryCode.Length + 1) : phoneNumber;
        }
        catch (NumberParseException)
        {
            return phoneNumber; // If parsing fails, return the original number
        }
    }

    public static string ExtractCountryCode(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return "N/A"; // No country code found

        var phoneUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();

        try
        {
            var number = phoneUtil.Parse(phoneNumber, null);
            return $"+{number.CountryCode}";
        }
        catch (NumberParseException)
        {
            return "Unknown"; // If parsing fails, return "Unknown"
        }
    }
}
