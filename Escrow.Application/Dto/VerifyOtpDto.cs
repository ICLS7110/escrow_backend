using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Application.Dto
{
    public class VerifyOtpDto
    {
        public string MobileNumber { get; set; }
        public string Otp { get; set; }
    }
}
