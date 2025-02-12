using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.ResultHandler;
public class Result<T>
{
    public bool IsSuccess { get; private set; } = false;
    public T? Value { get; private set; }    
    public string? ErrorMessage { get; private set; }= string.Empty;

    // Private constructor to enforce the use of factory methods
    private Result() { }

    public static Result<T> Success(T value)
    {
        return new Result<T>
        {
            IsSuccess = true,
            Value = value,            
            ErrorMessage = null
        };
    }

    public static Result<T> Failure(string? error)
    {
        return new Result<T>
        {            
            IsSuccess = false,
            ErrorMessage = error
        };
    }
}
