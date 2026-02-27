using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace API.Middleware;

/// <summary>
/// Middleware для централизованной обработки необработанных исключений.
/// </summary>
/// <remarks>
/// Перехватывает все исключения, возникающие в процессе обработки HTTP-запросов,
/// и формирует ответ в формате <c>application/problem+json</c> согласно RFC 7807.
/// В режиме разработки (<c>Development</c>) в ответ включается полная трассировка стека.
/// </remarks>
public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IWebHostEnvironment _env;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="GlobalExceptionHandler"/>.
    /// </summary>
    /// <param name="next">Следующий делегат в конвейере обработки запросов.</param>
    /// <param name="logger">Экземпляр логгера для записи сведений об исключениях.</param>
    /// <param name="env">Сведения о среде выполнения приложения.</param>
    public GlobalExceptionHandler(
        RequestDelegate next,
        ILogger<GlobalExceptionHandler> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    /// <summary>
    /// Вызывает следующий middleware в конвейере и перехватывает возникающие исключения.
    /// </summary>
    /// <param name="context">Контекст текущего HTTP-запроса.</param>
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

    /// <summary>
    /// Формирует HTTP-ответ с описанием ошибки на основе типа перехваченного исключения.
    /// </summary>
    /// <param name="context">Контекст текущего HTTP-запроса.</param>
    /// <param name="exception">Перехваченное исключение.</param>
    /// <remarks>
    /// Соответствие типов исключений HTTP-статусам:
    /// <list type="bullet">
    ///   <item><see cref="ValidationException"/> — 400 Bad Request с перечнем ошибок валидации.</item>
    ///   <item><see cref="UnauthorizedAccessException"/> — 401 Unauthorized.</item>
    ///   <item><see cref="InvalidOperationException"/> — 409 Conflict.</item>
    ///   <item>Прочие исключения — 500 Internal Server Error.</item>
    /// </list>
    /// </remarks>
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