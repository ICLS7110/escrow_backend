using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Entities;
    public class User : BaseAuditableEntity
    {    
        public string UserId { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? DateOfBirth { get; set; }

        // Business Fields
        public string? BusinessManagerName { get; set; }
        public string? BusinessEmail { get; set; }
        public string? CompanyEmail { get; set; }
        public string? VatId { get; set; }
        //public byte[]? ProofOfBusiness { get; set; } // File upload
        public string? LoginMethod { get; set; }
        public string? BusinessProof {  get; set; }
        public string? ProfilePicture {  get; set; }

        public bool IsProfileCompleted { get; set; } = false;
    }
