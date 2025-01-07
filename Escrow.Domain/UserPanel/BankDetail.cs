using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Domain.UserPanel
{
    public class BankDetail
    {
        public int Id { get; set; }
        public string AccountHolderName { get; set; }
        public string BankName { get; set; }
        public string IBANNumber { get; set; }
        public string BICCode { get; set; }
    }
}
