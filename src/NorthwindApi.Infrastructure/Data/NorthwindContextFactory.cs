using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NorthwindApi.Infrastructure.Data;

/// <summary>
/// Fábrica en tiempo de diseño para las herramientas de EF Core (migraciones/scaffolding).
/// La cadena de conexión se resuelve así, en orden de precedencia:
/// 1) variable de entorno <c>ConnectionStrings__Northwind</c>;
/// 2) variable de entorno <c>NORTHWIND_CONNECTION</c>;
/// 3) LocalDB por defecto (solo para desarrollo local).
/// Nunca se codifica una credencial en el código fuente.
/// </summary>
public sealed class NorthwindContextFactory : IDesignTimeDbContextFactory<Generated.NorthwindContext>
{
    private const string LocalDbFallback =
        "Server=(localdb)\\MSSQLLocalDB;Database=Northwind;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True";

    public Generated.NorthwindContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__Northwind")
            ?? Environment.GetEnvironmentVariable("NORTHWIND_CONNECTION")
            ?? LocalDbFallback;

        var options = new DbContextOptionsBuilder<Generated.NorthwindContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new Generated.NorthwindContext(options);
    }
}
