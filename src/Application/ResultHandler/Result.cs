using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.ResultHandler;

//TODO: we should have only one Result class not two 

public class Result<T>
{
    public int Status { get; private set; } 
    public T? Value { get; private set; }    
    public string? Message { get; private set; }= string.Empty;

    // Private constructor to enforce the use of factory methods
    private Result() { }

    public static Result<T> Success(int statuscode,string message,T value)
    {
        return new Result<T>
        {
            Status = statuscode,
            Value = value,
            Message = message
        };
    }

    public static Result<T> Failure(int statuscode,string error)
    {
        return new Result<T>
        {
            Status = statuscode,
            Message = error
        };
    }
}
