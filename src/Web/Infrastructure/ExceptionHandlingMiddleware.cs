using System.Diagnostics.Eventing.Reader;
using Escrow.Api.Application;
using Newtonsoft.Json;
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
            var response = new
            {
                Status = StatusCodes.Status500InternalServerError,
                Message = "An unhandled exception has occurred.",
                Value= new object() { }
            };
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            if (ex is EscrowApiException customValidationException)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new
                {
                    Status = StatusCodes.Status400BadRequest,
                    Message = customValidationException.Message,
                    Value = new object() { }
                };
            }

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}


