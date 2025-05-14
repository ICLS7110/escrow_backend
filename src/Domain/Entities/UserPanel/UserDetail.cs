using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Entities.UserPanel;
public class UserDetail : BaseAuditableEntity
{    
    public string UserId { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? EmailAddress { get; set; }
    public string? Password { get; set; }
    public string? Role { get; set; }
    public string? OTP { get; set; }
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
    public string? AccountType {  get; set; } 
    public bool IsProfileCompleted { get; set; } = false;
    public string? DeviceToken { get; set; } // Store Firebase Device Token
    public string? SocialId { get; set; } // Store Firebase Device Token
    public bool IsDeleted { get; set; } = false;
    public bool? IsActive { get; set; } = false;
    public bool? IsNotified { get; set; } = false;
    //public string? Language { get; set; } = string.Empty;
}
