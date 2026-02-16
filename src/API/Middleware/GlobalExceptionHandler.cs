using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace API.Middleware;

public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionHandler(
        RequestDelegate next, 
        ILogger<GlobalExceptionHandler> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = (int)HttpStatusCode.InternalServerError,
            Title = "An error occurred while processing your request.",
            Type = "https://httpstatuses.io/500",
        };

        switch (exception)
        {
            case ValidationException validationException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errors = validationException.Errors
                    .Select(e => e.ErrorMessage).ToArray();
                var bad = new ValidationProblemDetails();

                foreach (var err in errors)
                {
                    bad.Errors.Add("Validation", new[] { err });
                }

                bad.Status = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(JsonSerializer.Serialize(bad));
                return;

            case UnauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                problem.Status = (int)HttpStatusCode.Unauthorized;
                problem.Title = "Unauthorized";
                break;

            case InvalidOperationException:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                problem.Status = (int)HttpStatusCode.Conflict;
                problem.Title = "Conflict";
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                problem.Status = (int)HttpStatusCode.InternalServerError;
                break;
        }

        if (_env.IsDevelopment())
        {
            problem.Detail = exception.ToString();
        }

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}