using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.UserPanel;

namespace Escrow.Api.Application.Authentication.Interfaces;
public  interface IOtpManagerService
{
    Task<bool> RequestOtpAsync(string countryCode,string phoneNumber);
    Task<Result<UserDetail>> VerifyOtpAsync(string countryCode,string phoneNumber, string otp);
}
