using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application;
public class CustomValidationException : Exception
{
    public CustomValidationException(string errorMessage)
            : base(errorMessage)
    {
    }
}
