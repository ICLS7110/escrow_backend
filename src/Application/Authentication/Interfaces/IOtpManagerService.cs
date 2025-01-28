using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Domain.Entities.UserPanel;

namespace Escrow.Api.Application.Authentication.Interfaces;
public  interface IOtpManagerService
{
    Task RequestOtpAsync(string countryCode,string phoneNumber);
    Task<UserDetail> VerifyOtpAsync(string countryCode,string phoneNumber, string otp);
}
