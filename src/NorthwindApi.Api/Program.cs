using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using NorthwindApi.Api.Middleware;
using NorthwindApi.Infrastructure;
using NorthwindApi.Infrastructure.Data.Generated;

var builder = WebApplication.CreateBuilder(args);

// Cadena de conexión: User Secrets (dev) o variables de entorno / appsettings (prod).
// Nunca se codifican credenciales en el repositorio.
var connectionString = builder.Configuration.GetConnectionString("Northwind")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__Northwind")
    ?? throw new InvalidOperationException(
        "Falta la cadena de conexión 'ConnectionStrings:Northwind'. " +
        "Configúrela con User Secrets, una variable de entorno o appsettings.");

builder.Services.AddInfrastructure(connectionString);

builder.Services
    .AddControllers(options =>
    {
        options.ReturnHttpNotAcceptable = false;
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    // Los errores de DataAnnotations se devuelven como ProblemDetails consistente.
    options.InvalidModelStateResponseFactory = context =>
    {
        var problem = new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Datos inválidos",
            Detail = "Uno o más errores de validación ocurrieron.",
            Instance = context.HttpContext.Request.Path
        };
        problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        return new BadRequestObjectResult(problem);
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Northwind API",
        Version = "v1",
        Description = "API REST sobre la base de datos Northwind (SQL Server). " +
                      "CRUD de entidades, vistas de solo lectura, procedimientos almacenados, " +
                      "métricas de dashboard y exportación a PDF/XLSX."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

const string CorsPolicy = "web";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

app.UseAppExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Northwind API v1"));
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors(CorsPolicy);

app.MapControllers();

// Comprobación de salud: verifica conectividad real con SQL Server.
app.MapGet("/health", async (NorthwindContext db) =>
{
    var canConnect = await db.Database.CanConnectAsync();
    return canConnect
        ? Results.Ok(new { status = "Healthy", database = "Northwind" })
        : Results.Problem("No se pudo conectar con la base de datos.", statusCode: 503);
})
.WithName("HealthCheck")
.WithTags("Salud");

app.Run();

/// <summary>Punto de entrada expuesto para las pruebas de integración.</summary>
public partial class Program;
