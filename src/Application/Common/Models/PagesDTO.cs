using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Models;
public class PagesDTO
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public string? Content { get; set; }
    public bool? IsActive { get; set; }
    public DateTime Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
    public int RecordState { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
}
