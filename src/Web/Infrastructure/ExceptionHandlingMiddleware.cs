using System.Diagnostics.Eventing.Reader;
using System.Net;
using Escrow.Api.Application;
using Newtonsoft.Json;
using Twilio.TwiML.Messaging;
using YamlDotNet.Core.Tokens;

namespace Escrow.Api.Web.Infrastructure;

public class ExceptionHandlingMiddleware : IMiddleware
{

    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate _next)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await ExceptionHandlerAsync(context, ex);
        }
    }


    private async Task ExceptionHandlerAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode status;
        string message;
        switch (exception)
        {
            case EscrowDataNotFoundException:
                status = HttpStatusCode.NotFound;
                message = exception.Message;
                break;
            case EscrowValidationException:
                status = HttpStatusCode.BadRequest;
                message = exception.Message;
                break;
            case EscrowUnauthorizedAccessException:
                status = HttpStatusCode.Unauthorized;
                message = "Unauthorized access.";
                break;
            case EscrowForbiddenException:
                status = HttpStatusCode.Forbidden;
                message = exception.Message;
                break;
            case EscrowConflictException:
                status = HttpStatusCode.Conflict;
                message = exception.Message;
                break;
            case TimeoutException:
                status = HttpStatusCode.RequestTimeout;
                message = exception.Message;
                break;
            case EscrowRecordCreationException:
                status = HttpStatusCode.BadRequest;
                message = exception.Message;
                break;
            default:
                status = HttpStatusCode.InternalServerError;
                message = "An unexpected error occurred.";
                break;
        }
        var response = new
        {
            Status = status,
            Message = message,
            Value = new object() { }
        };
        await context.Response.WriteAsJsonAsync(response);
    }
}


