using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Authentication.Interfaces;
public  interface IOtpManagerService
{
    Task RequestOtpAsync(string countryCode,string phoneNumber);
    Task<string> VerifyOtpAsync(string countryCode,string phoneNumber, string otp);
}
