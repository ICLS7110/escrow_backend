using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Interfaces;
public interface IOtpValidationService
{
    bool ValidatePhoneNumber(string phoneNumber);
}
