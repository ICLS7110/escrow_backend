using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation.Results;

namespace Escrow.Api.Application;
public class EscrowApiException : Exception
{
    public EscrowApiException(string errorMessage)
            : base(errorMessage)
    {
    }
}
public class EscrowDataNotFoundException : Exception
{
    public EscrowDataNotFoundException(string errorMessage) : base(errorMessage)
    {
            
    }
}
public class EscrowUnauthorizedAccessException :  Exception
{
    public EscrowUnauthorizedAccessException(string errorMessage) : base(errorMessage)
    {

    }
}

public class EscrowForbiddenException : Exception
{
    public EscrowForbiddenException(string errorMessage) : base(errorMessage)
    {

    }
}
public class EscrowConflictException : Exception
{
    public EscrowConflictException(string errorMessage) : base(errorMessage)
    {

    }
}

public class EscrowValidationException : Exception
{    

    public EscrowValidationException(string errorMessage) : base(errorMessage)
    {
            
    }    
}

public class EscrowRecordCreationException : Exception
{

    public EscrowRecordCreationException(string errorMessage) : base(errorMessage)
    {

    }
}
