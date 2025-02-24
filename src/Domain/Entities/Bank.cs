using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Entities;
public class Bank: BaseAuditableEntity
{
    public int UserDetailId { get; set; }
    public User UserDetail { get; set; } = null!;

    public string AccountHolderName { get; set; } = String.Empty;
    public string IBANNumber { get; set; } = String.Empty;
    public string BICCode { get; set; } = String.Empty;
    public string BankName {  get; set; } = String.Empty;

}
