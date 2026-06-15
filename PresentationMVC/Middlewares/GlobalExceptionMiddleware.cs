using Application.Common.Responses;
using Application.Exceptions;
using FluentValidation;

namespace Presentation.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        int statusCode;
        object response;

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = 400;
                response = new
                {
                    success = false,
                    statusCode,
                    errors = validationException.Errors
                        .Select(e => new
                        {
                            field = e.PropertyName,
                            message = e.ErrorMessage
                        })
                };
                break;

            case BaseException baseException:
                statusCode = baseException.StatusCode;
                response = ApiResponse<object>.Fail(statusCode, baseException.Message);
                break;

            default:
                statusCode = 500;
                response = ApiResponse<object>.Fail(500, "An unexpected error occurred.");
                break;
        }

        if (statusCode >= 500)
            _logger.LogError(exception, "Unhandled Exception: {Message}", exception.Message);
        else
            _logger.LogWarning("Handled Exception [{StatusCode}]: {Message}", statusCode, exception.Message);

        // ✅ لو MVC request (مش API) — اعمل redirect بدل JSON
        if (!IsApiRequest(context))
        {
            // خزن الـ error message في TempData عشان تعرضه في الـ View
            context.Response.Redirect($"/Error?statusCode={statusCode}&message={Uri.EscapeDataString(exception.Message)}");
            return;
        }

        // ✅ لو API request — رد بـ JSON
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(response);
    }

    // ✅ بيحدد هل الـ request ده API ولا MVC
    private static bool IsApiRequest(HttpContext context)
    {
        return context.Request.Path.StartsWithSegments("/api") ||
               context.Request.Headers["Accept"].ToString().Contains("application/json") ||
               context.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
    }
}