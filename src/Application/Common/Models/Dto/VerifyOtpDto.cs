using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Models.Dto;
public class VerifyOtpDto
{
    public required string CountryCode { get; set; }
    public required string MobileNumber { get; set; }
    public required string Otp { get; set; }
}
