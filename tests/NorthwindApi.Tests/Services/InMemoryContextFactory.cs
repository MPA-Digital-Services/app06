using Microsoft.EntityFrameworkCore;
using NorthwindApi.Infrastructure.Data.Generated;

namespace NorthwindApi.Tests.Services;

/// <summary>
/// Crea instancias aisladas de <see cref="NorthwindContext"/> sobre el proveedor
/// InMemory (nombre de base de datos único por instancia).
/// </summary>
internal static class InMemoryContextFactory
{
    public static NorthwindContext Create()
    {
        var options = new DbContextOptionsBuilder<NorthwindContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new NorthwindContext(options);
    }
}
