using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OrderIntegration.Api.Domain.Exceptions;

namespace OrderIntegration.Api.Middleware;

/// <summary>
/// Middleware para manejo global de excepciones con ProblemDetails.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        var (statusCode, problemDetails) = CreateProblemDetails(context, exception);

        _logger.LogError(
            exception,
            "Error no manejado: {ErrorType} - {Message}. TraceId: {TraceId}",
            exception.GetType().Name,
            exception.Message,
            problemDetails.Extensions["traceId"]);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, JsonOptions));
    }

    private (int StatusCode, ProblemDetails ProblemDetails) CreateProblemDetails(
        HttpContext context, 
        Exception exception)
    {
        var traceId = context.TraceIdentifier;

        return exception switch
        {
            ConflictException ex => (
                (int)HttpStatusCode.Conflict,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.Conflict,
                    Title = "Conflicto de negocio",
                    Detail = ex.Message,
                    Type = "https://httpstatuses.com/409",
                    Instance = context.Request.Path,
                    Extensions = { ["traceId"] = traceId }
                }),

            NotFoundException ex => (
                (int)HttpStatusCode.NotFound,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.NotFound,
                    Title = "Recurso no encontrado",
                    Detail = ex.Message,
                    Type = "https://httpstatuses.com/404",
                    Instance = context.Request.Path,
                    Extensions = { ["traceId"] = traceId }
                }),

            InvalidOperationException ex => (
                (int)HttpStatusCode.Conflict,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.Conflict,
                    Title = "Operación no válida",
                    Detail = ex.Message,
                    Type = "https://httpstatuses.com/409",
                    Instance = context.Request.Path,
                    Extensions = { ["traceId"] = traceId }
                }),

            ArgumentException ex => (
                (int)HttpStatusCode.BadRequest,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = "Solicitud inválida",
                    Detail = ex.Message,
                    Type = "https://httpstatuses.com/400",
                    Instance = context.Request.Path,
                    Extensions = { ["traceId"] = traceId }
                }),

            KeyNotFoundException ex => (
                (int)HttpStatusCode.NotFound,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.NotFound,
                    Title = "Recurso no encontrado",
                    Detail = ex.Message,
                    Type = "https://httpstatuses.com/404",
                    Instance = context.Request.Path,
                    Extensions = { ["traceId"] = traceId }
                }),

            UnauthorizedAccessException ex => (
                (int)HttpStatusCode.Unauthorized,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.Unauthorized,
                    Title = "No autorizado",
                    Detail = ex.Message,
                    Type = "https://httpstatuses.com/401",
                    Instance = context.Request.Path,
                    Extensions = { ["traceId"] = traceId }
                }),

            _ => (
                (int)HttpStatusCode.InternalServerError,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Title = "Error interno del servidor",
                    Detail = context.RequestServices.GetService<IHostEnvironment>()?.IsDevelopment() == true
                        ? exception.Message
                        : "Ha ocurrido un error inesperado. Por favor, intente nuevamente.",
                    Type = "https://httpstatuses.com/500",
                    Instance = context.Request.Path,
                    Extensions = { ["traceId"] = traceId }
                })
        };
    }
}

/// <summary>
/// Extensiones para registrar el middleware.
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
