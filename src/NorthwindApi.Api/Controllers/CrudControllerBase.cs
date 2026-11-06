using Microsoft.AspNetCore.Mvc;
using NorthwindApi.Application.Abstractions;
using NorthwindApi.Application.Common;

namespace NorthwindApi.Api.Controllers;

/// <summary>
/// Controlador CRUD genérico. Cada entidad concreta fija su clave, DTOs y filtro,
/// y aporta un selector de clave para construir el encabezado Location.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class CrudControllerBase<TKey, TDto, TCreate, TUpdate, TFilter> : ControllerBase
    where TFilter : new()
{
    protected ICrudService<TKey, TDto, TCreate, TUpdate, TFilter> Service { get; }

    protected CrudControllerBase(ICrudService<TKey, TDto, TCreate, TUpdate, TFilter> service)
        => Service = service;

    /// <summary>Extrae la clave del DTO devuelto por la creación.</summary>
    protected abstract TKey GetKey(TDto dto);

    /// <summary>Lista paginada con filtro y orden.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<TDto>>> GetPaged(
        [FromQuery] PageQuery paging,
        [FromQuery] TFilter filter,
        CancellationToken ct)
        => Ok(await Service.GetPagedAsync(paging, filter, ct));

    /// <summary>Obtiene un recurso por su clave.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TDto>> GetById(TKey id, CancellationToken ct)
    {
        var dto = await Service.GetByIdAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    /// <summary>Crea un recurso.</summary>
    [HttpPost]
    public async Task<ActionResult<TDto>> Create([FromBody] TCreate dto, CancellationToken ct)
    {
        var created = await Service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = GetKey(created) }, created);
    }

    /// <summary>Actualiza un recurso existente.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(TKey id, [FromBody] TUpdate dto, CancellationToken ct)
    {
        var updated = await Service.UpdateAsync(id, dto, ct);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Elimina un recurso.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(TKey id, CancellationToken ct)
    {
        var deleted = await Service.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}
