using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Models;

namespace Escrow.Api.Application.Common.Interfaces;
public interface IAnbService
{
    Task<string> GetAccountVerificationStatusAsync(string requestReferenceNumber);
    Task<VerifyAccountResponse?> VerifyAccountAsync(VerifyAccountRequest request);

    Task<string?> GetAccessTokenAsync();
}
