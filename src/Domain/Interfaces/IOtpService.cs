using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Interfaces;
public interface IOtpService
{
    string GenerateOtp();
    void SendOtp(string phoneNumber, string otp);
}
