using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentValidation.Results;

namespace Escrow.Api.Application.Abstraction;
public abstract class EscrowApiExceptionBase : Exception
{

    public abstract HttpStatusCode StatusCode { get; }
    public EscrowApiExceptionBase(string errorMessage) : base(errorMessage)
    {
    }
}
