using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Entities.ContactUs;
public class ContactUs 
{

    public int Id { get; set; }
    public string FullName { get; set; }= string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset Created { get; set; }
}
