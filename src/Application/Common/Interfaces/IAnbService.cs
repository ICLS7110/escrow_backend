using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Interfaces;
public interface IAnbService
{
    Task<string> GetAccountBalanceAsync(string accountNumber);
}
