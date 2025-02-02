using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Entities.UserPanel;
public class BankDetail: BaseAuditableEntity
{
    public int UserDetailId { get; set; }
    public UserDetail UserDetail { get; set; } = null!;

    public string AccountHolderName { get; set; } = String.Empty;
    public string IBANNumber { get; set; } = String.Empty;
    public string BICCode { get; set; } = String.Empty;
}
