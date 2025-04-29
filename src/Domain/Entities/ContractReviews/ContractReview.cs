using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Entities.ContractReviews;
public class ContractReview : BaseAuditableEntity
{
    public int ContractId { get; set; }
    public int ReviewerId { get; set; } // User who writes the review
    public int RevieweeId { get; set; } // User being reviewed
    public string? Rating { get; set; } // Rating (1 to 5 stars)
    public string? ReviewText { get; set; } // Optional review text
    public bool? IsActive { get; set; } // Optional review text
    public bool? IsDeleted { get; set; } // Optional review text
}
