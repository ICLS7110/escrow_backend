using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Abstraction;

namespace Escrow.Api.Application.Exceptions;
public class EscrowDataNotFoundException : EscrowApiExceptionBase
{
    public EscrowDataNotFoundException(string errorMessage) : base(errorMessage)
    {

    }

    public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;
}
public class EscrowUnauthorizedAccessException : EscrowApiExceptionBase
{
    public EscrowUnauthorizedAccessException(string errorMessage) : base(errorMessage)
    {

    }

    public override HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;
}

public class EscrowForbiddenException : EscrowApiExceptionBase
{
    public EscrowForbiddenException(string errorMessage) : base(errorMessage)
    {

    }

    public override HttpStatusCode StatusCode => HttpStatusCode.Forbidden;
}
public class EscrowConflictException : EscrowApiExceptionBase
{
    public EscrowConflictException(string errorMessage) : base(errorMessage)
    {

    }

    public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;
}

public class EscrowValidationException : EscrowApiExceptionBase
{

    public EscrowValidationException(string errorMessage) : base(errorMessage)
    {

    }

    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
}

public class EscrowRecordCreationException : EscrowApiExceptionBase
{

    public EscrowRecordCreationException(string errorMessage) : base(errorMessage)
    {

    }

    public override HttpStatusCode StatusCode => HttpStatusCode.BadGateway;
}
