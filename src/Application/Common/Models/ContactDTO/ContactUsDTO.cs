using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Models.ContactDTO;
public class ContactUsDTO
{
    public string? Name { get; set; }
    public string? Number { get; set; }
    public string? Email { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
}
