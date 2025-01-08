using Escrow.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Domain.Entities.UserPanel
{
    public class User : BaseAuditableEntity
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string MobileNumber { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }

        // Business Fields
        public string BusinessManagerName { get; set; }
        public string BusinessEmail { get; set; }
        public string VatId { get; set; }
        public byte[] ProofOfBusiness { get; set; } // File as binary

        // Bank Account Details Fields
        public string AccountHolderName { get; set; }
        public string IBANNumber { get; set; }
        public string BICCode { get; set; }

        public string LoginMethod { get; set; }
    }
}
