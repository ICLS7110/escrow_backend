using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Models;
//public class SMSRequest
//{
//    public string? AppSid { get; set; }
//    public string? Recipient { get; set; }
//    public string? Body { get; set; }
//    public string SenderID { get; set; } = "DevWePayBE"; // Optional sender name
//}

public class SMSRequest
{
    public string To { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
