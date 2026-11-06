using Microsoft.EntityFrameworkCore;
using NorthwindApi.Application.Abstractions;
using NorthwindApi.Application.Common;
using NorthwindApi.Infrastructure.Data.Generated;

namespace NorthwindApi.Infrastructure.Services;

/// <summary>
/// Implementación base de CRUD genérico sobre EF Core.
/// - Las consultas de lectura usan <c>AsNoTracking()</c> (ver <see cref="BuildQuery"/>).
/// - La búsqueda por clave usa el conjunto rastreado para permitir update/delete.
/// - Las violaciones de FK/unicidad se traducen a errores HTTP en la capa API.
/// </summary>
public abstract class CrudServiceBase<TEntity, TKey, TDto, TCreate, TUpdate, TFilter>
    : ICrudService<TKey, TDto, TCreate, TUpdate, TFilter>
    where TEntity : class
{
    protected readonly NorthwindContext Db;

    protected CrudServiceBase(NorthwindContext db) => Db = db;

    protected DbSet<TEntity> Set => Db.Set<TEntity>();

    /// <summary>Consulta de lectura sin seguimiento, ya filtrada.</summary>
    protected abstract IQueryable<TEntity> BuildQuery(TFilter filter);

    protected abstract IQueryable<TEntity> ApplySort(IQueryable<TEntity> query, PageQuery paging);

    protected abstract Task<TEntity?> FindByIdAsync(TKey id, CancellationToken ct);

    protected abstract TDto Map(TEntity entity);

    protected abstract TEntity ToEntity(TCreate dto);

    protected abstract void Apply(TEntity entity, TUpdate dto);

    /// <summary>Extensión para validar referencias (FK) antes de guardar.</summary>
    protected virtual Task ValidateReferencesAsync(TEntity entity, CancellationToken ct) => Task.CompletedTask;

    public virtual async Task<PagedResult<TDto>> GetPagedAsync(PageQuery paging, TFilter filter, CancellationToken ct = default)
    {
        var query = BuildQuery(filter);
        var total = await query.CountAsync(ct);
        var items = await ApplySort(query, paging)
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .ToListAsync(ct);

        return new PagedResult<TDto>
        {
            Items = items.Select(Map).ToList(),
            Page = paging.Page,
            PageSize = paging.PageSize,
            TotalCount = total
        };
    }

    public virtual async Task<TDto?> GetByIdAsync(TKey id, CancellationToken ct = default)
    {
        var entity = await FindByIdAsync(id, ct);
        if (entity is null)
        {
            return default;
        }

        var dto = Map(entity);
        Db.Entry(entity).State = EntityState.Detached;
        return dto;
    }

    public virtual async Task<TDto> CreateAsync(TCreate dto, CancellationToken ct = default)
    {
        var entity = ToEntity(dto);
        await ValidateReferencesAsync(entity, ct);
        Set.Add(entity);
        await Db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public virtual async Task<bool> UpdateAsync(TKey id, TUpdate dto, CancellationToken ct = default)
    {
        var entity = await FindByIdAsync(id, ct);
        if (entity is null)
        {
            return false;
        }

        Apply(entity, dto);
        await ValidateReferencesAsync(entity, ct);
        await Db.SaveChangesAsync(ct);
        return true;
    }

    public virtual async Task<bool> DeleteAsync(TKey id, CancellationToken ct = default)
    {
        var entity = await FindByIdAsync(id, ct);
        if (entity is null)
        {
            return false;
        }

        Set.Remove(entity);
        await Db.SaveChangesAsync(ct);
        return true;
    }

    protected static IQueryable<T> Sort<T>(
        IQueryable<T> query,
        PageQuery paging,
        IReadOnlyDictionary<string, System.Linq.Expressions.Expression<Func<T, object>>> map,
        System.Linq.Expressions.Expression<Func<T, object>> defaultSort)
        => query.ApplySort(paging, map, defaultSort);
}
