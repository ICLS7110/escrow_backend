using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Models;
public class UserLoginDto
{
    public string UserId { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? EmailAddress { get; set; }
    public string Token { get; set; } = string.Empty;
}
