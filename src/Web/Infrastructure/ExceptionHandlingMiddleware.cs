using System.Diagnostics.Eventing.Reader;
using Escrow.Api.Application;
using Newtonsoft.Json;

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
                IsSuccess = false,
                ErrorMessage = "An unhandled exception has occurred."
            };
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            if (ex is CustomValidationException customValidationException)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new
                {
                    IsSuccess = false,
                    ErrorMessage = customValidationException.Message
                };
            }

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}


