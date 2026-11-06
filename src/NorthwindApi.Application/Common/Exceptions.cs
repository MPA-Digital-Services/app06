namespace NorthwindApi.Application.Common;

/// <summary>Recurso solicitado inexistente -> 404.</summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

/// <summary>Conflicto de unicidad o de clave foránea -> 409.</summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

/// <summary>Entrada inválida detectada en la capa de aplicación -> 400.</summary>
public sealed class AppValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public AppValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]> { ["general"] = new[] { message } };
    }

    public AppValidationException(IDictionary<string, string[]> errors)
        : base("Uno o más errores de validación ocurrieron.")
    {
        Errors = errors;
    }
}
