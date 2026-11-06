using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NorthwindApi.Application.Common;

namespace NorthwindApi.Api.Middleware;

/// <summary>
/// Traduce excepciones de dominio y de SQL Server a respuestas ProblemDetails (RFC 7807)
/// con el código HTTP adecuado, evitando filtrar detalles internos en producción.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
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
        catch (AppValidationException ex)
        {
            _logger.LogInformation(ex, "Validación fallida en {Path}", context.Request.Path);
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Datos inválidos",
                "Uno o más errores de validación ocurrieron.", ex.Errors);
        }
        catch (NotFoundException ex)
        {
            _logger.LogInformation(ex, "Recurso no encontrado en {Path}", context.Request.Path);
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, "Recurso no encontrado", ex.Message);
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning(ex, "Conflicto en {Path}", context.Request.Path);
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Conflicto", ex.Message);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Conflicto de concurrencia en {Path}", context.Request.Path);
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Conflicto de concurrencia",
                "El recurso fue modificado o eliminado por otro proceso.");
        }
        catch (DbUpdateException ex)
        {
            var (status, title, detail) = MapDbUpdate(ex);
            _logger.LogWarning(ex, "Error de base de datos en {Path}", context.Request.Path);
            await WriteProblemAsync(context, status, title, detail);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error no controlado en {Path}", context.Request.Path);
            var detail = _env.IsDevelopment()
                ? ex.ToString()
                : "Ocurrió un error inesperado al procesar la solicitud.";
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError,
                "Error interno del servidor", detail);
        }
    }

    private static (int Status, string Title, string Detail) MapDbUpdate(DbUpdateException ex)
    {
        if (ex.InnerException is SqlException sql)
        {
            return sql.Number switch
            {
                547 => (StatusCodes.Status409Conflict, "Restricción de integridad",
                    "La operación viola una clave foránea o una restricción de la base de datos."),
                2601 or 2627 => (StatusCodes.Status409Conflict, "Valor duplicado",
                    "Ya existe un registro con la misma clave o valor único."),
                515 => (StatusCodes.Status400BadRequest, "Valor requerido",
                    "Falta un valor obligatorio para una columna que no admite nulos."),
                2628 or 8152 => (StatusCodes.Status400BadRequest, "Longitud excedida",
                    "Uno de los valores supera la longitud máxima permitida."),
                _ => (StatusCodes.Status409Conflict, "Error de base de datos",
                    "La operación no pudo completarse por una restricción de la base de datos.")
            };
        }

        return (StatusCodes.Status409Conflict, "Error de base de datos",
            "La operación no pudo completarse por una restricción de la base de datos.");
    }

    private static async Task WriteProblemAsync(
        HttpContext context, int status, string title, string detail,
        IDictionary<string, string[]>? errors = null)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;

        if (errors is { Count: > 0 })
        {
            problem.Extensions["errors"] = errors;
        }

        await context.Response.WriteAsJsonAsync(problem);
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseAppExceptionHandling(this IApplicationBuilder app) =>
        app.UseMiddleware<ExceptionHandlingMiddleware>();
}
