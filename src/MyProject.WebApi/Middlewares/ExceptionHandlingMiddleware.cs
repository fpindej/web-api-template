using System.Net;
using System.Text.Json;
using MyProject.Infrastructure.Features.Postgres.Exceptions;
using MyProject.WebApi.Shared;

namespace MyProject.WebApi.Middlewares;

public class ExceptionHandlingMiddleware(
    RequestDelegate _next,
    ILogger<ExceptionHandlingMiddleware> _logger,
    IHostEnvironment _env)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next.Invoke(context);
        }
        catch (KeyNotFoundException keyNotFoundEx)
        {
            _logger.LogWarning(keyNotFoundEx, "A KeyNotFoundException occurred.");
            await HandleExceptionAsync(context, keyNotFoundEx, HttpStatusCode.NotFound);
        }
        catch (PaginationException paginationEx)
        {
            _logger.LogWarning(paginationEx, "A PaginationException occurred.");
            await HandleExceptionAsync(context, paginationEx, HttpStatusCode.BadRequest);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, e, HttpStatusCode.InternalServerError);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        HttpStatusCode statusCode,
        string? customMessage = null)
    {
        var errorResponse = new ErrorResponse
        {
            Message = customMessage ?? exception.Message,
            Details = _env.IsDevelopment() ? exception.StackTrace : null
        };

        var payload = JsonSerializer.Serialize(errorResponse);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(payload);
    }
}
