using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Models
{

    public class CustomerDto
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public string? DateOfBirth { get; set; }
        public string? BusinessManagerName { get; set; }
        public string? BusinessEmail { get; set; }
        public string? VatId { get; set; }
        public string? LoginMethod { get; set; }
        public string? Created { get; set; }
        public string? CreatedBy { get; set; }
        public string? LastModified { get; set; }
        public string? LastModifiedBy { get; set; }
        public string? BusinessProof { get; set; }
        public string? ProfilePicture { get; set; }
        public bool? IsProfileCompleted { get; set; }
        public string? CompanyEmail { get; set; }
        public bool? IsActive { get; set; }
    }
}
