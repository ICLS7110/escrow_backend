using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Models;
public class ContractReviewDTO
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public int ReviewerId { get; set; }
    public int RevieweeId { get; set; }
    public string? Rating { get; set; }  // Rating out of 5 stars
    public string? ReviewText { get; set; }
    public bool? IsActive { get; set; }
}
