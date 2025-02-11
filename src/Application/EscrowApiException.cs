using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application;
public class EscrowApiException : Exception
{
    public EscrowApiException(string errorMessage)
            : base(errorMessage)
    {
    }
}
