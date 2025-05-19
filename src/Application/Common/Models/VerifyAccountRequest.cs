using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Models;
public class VerifyAccountRequest
{
    public string? Iban { get; set; }
    public string? NationalId { get; set; }
    public string? DestinationBankBIC { get; set; }
    public string? AuthToken { get; set; }

}

