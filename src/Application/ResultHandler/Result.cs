
using System.Text.Json.Serialization;

namespace Escrow.Api.Application.DTOs;

public class Result<T>
{
    public int Status { get; private set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; private set; }
    public string Message { get; private set; } = string.Empty;


    private Result() { }

    public static Result<T> Success(int statusCode, string message, T? value = default)
    {
        return new Result<T>
        {
            Status = statusCode,
            Data = value,
            Message = message
        };
    }

    public static Result<T> Failure(int statuscode, string error)
    {
        return new Result<T>
        {
            Status = statuscode,
            Message = error
        };
    }
}
