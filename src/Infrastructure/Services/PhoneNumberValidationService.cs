using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Domain.Interfaces;

namespace Escrow.Api.Infrastructure.Services;
public class PhoneNumberValidationService : IOtpValidationService
{
    public bool ValidatePhoneNumber(string phoneNumber)
    {
        return !string.IsNullOrWhiteSpace(phoneNumber) && phoneNumber.All(char.IsDigit) && phoneNumber.Length == 10;
    }
}
