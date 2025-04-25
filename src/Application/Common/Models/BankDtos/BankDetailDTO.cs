using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Models.BankDtos;
public class BankDetailDTO
{
    public int Id { get; set; }
    public string AccountHolderId { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
    public string IBANNumber { get; set; } = string.Empty;
    public string BICCode { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
}
