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

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        context.Response.ContentType = "application/json";

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

                response = ApiResponse<object>.Fail(
                    statusCode,
                    baseException.Message);

                break;

            default:

                statusCode = 500;

                response = ApiResponse<object>.Fail(
                    500,
                    "An unexpected error occurred.");

                break;
        }

        if (statusCode >= 500)
        {
            _logger.LogError(
                exception,
                "Unhandled Exception");
        }
        else
        {
            _logger.LogWarning(
                exception.Message);
        }

        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsJsonAsync(response);
    }

}