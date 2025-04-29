using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Interfaces;
public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject,string Name, string body);
}
