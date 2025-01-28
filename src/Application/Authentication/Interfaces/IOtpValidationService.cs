using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Authentication.Interfaces;
public interface IOtpValidationService
{
    Task<bool> ValidatePhoneNumberAsync(string phoneNumber);
}
