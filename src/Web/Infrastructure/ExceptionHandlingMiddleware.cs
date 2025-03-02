using System.Diagnostics.Eventing.Reader;
using System.Net;
using Escrow.Api.Application.Abstraction;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.ResultHandler;
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
        catch (Exception e) {

            await ExceptionHandlerAsync(context, e);
        }
    }

  
    private async Task ExceptionHandlerAsync(HttpContext context, Exception exception)
    {
        Result<object> result;
        if (exception is EscrowApiExceptionBase escrowEx)
        {
            result = Result<object>.Failure((int)escrowEx.StatusCode, escrowEx.Message);
        }
        else
        {
            result = Result<object>.Failure(500, "Unexpected Server Error.");

        }

        _logger.LogError(exception, exception.Message);

        context.Response.StatusCode = result.Status;
        await context.Response.WriteAsJsonAsync(result);

    }
}


