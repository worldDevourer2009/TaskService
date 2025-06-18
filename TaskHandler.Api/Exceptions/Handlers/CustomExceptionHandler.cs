using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskHandler.Application.Exceptions;

namespace TaskHandler.Api.Exceptions.Handlers;

public class CustomExceptionHandler : IExceptionHandler
{
    private readonly ILogger<CustomExceptionHandler> _logger;

    public CustomExceptionHandler(ILogger<CustomExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError($"Error {exception.Message}, time {DateTime.UtcNow}");

        (string details, string title, int statusCode) details = exception switch
        {
            ValidationException validationException => (
                string.Join("; ", validationException.Errors.Select(e => e.ErrorMessage)),
                "Validation Error",
                StatusCodes.Status400BadRequest),
            TaskItemNotFoundException => (exception.Message, exception.GetType().Name, StatusCodes.Status404NotFound),
            _ => (exception.Message,
                exception.GetType().Name,
                StatusCodes.Status500InternalServerError),
        };

        var problemDetails = new ProblemDetails
        {
            Detail = details.details,
            Title = details.title,
            Status = details.statusCode,
            Instance = httpContext.Request.Path
        };
        
        if (exception is ValidationException validationEx)
        {
            problemDetails.Extensions["errors"] = validationEx.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );
        }
        
        httpContext.Response.StatusCode = details.statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        
        return true;
    }
}