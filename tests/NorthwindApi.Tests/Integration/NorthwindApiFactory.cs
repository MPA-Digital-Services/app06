using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace NorthwindApi.Tests.Integration;

/// <summary>
/// Fábrica de la API en memoria para pruebas de integración. Usa la base de datos
/// Northwind real. La cadena de conexión se resuelve así:
/// 1) variable de entorno <c>NORTHWIND_TEST_CONNECTION</c>;
/// 2) LocalDB por defecto (SQL Server LocalDB en Windows).
/// </summary>
public sealed class NorthwindApiFactory : WebApplicationFactory<Program>
{
    public const string DefaultConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=Northwind;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True";

    public static string ConnectionString =>
        Environment.GetEnvironmentVariable("NORTHWIND_TEST_CONNECTION") ?? DefaultConnectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Northwind", ConnectionString);
    }
}
