using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Models;
public class ANBSettings
{
    public string? BaseUrl { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? TokenEndpoint { get; set; }
    public string? AnbCookie { get; set; }
}


public class VerifyAccountResponse
{
    public string? Id { get; set; }
    public string? RequestReferenceNumber { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
