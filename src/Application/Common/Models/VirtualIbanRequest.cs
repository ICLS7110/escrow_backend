using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Models;
public class VirtualIbanRequest
{
    public long CustomerId { get; set; }
    public int CompanyId { get; set; }
    public string BankCode { get; set; } = string.Empty;
}

