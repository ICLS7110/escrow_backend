using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Interfaces;
public interface IOtpManagerService
{
    Task RequestOtpAsync(string phoneNumber);
    Task<string> VerifyOtpAsync(string phoneNumber, string otp);
}
