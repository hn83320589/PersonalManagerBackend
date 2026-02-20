using PersonalManager.Api.Repositories;

namespace PersonalManager.Api.Services;

public interface ICrudService<T, TCreate, TUpdate, TResponse> where T : class
{
    Task<List<TResponse>> GetAllAsync();
    Task<TResponse?> GetByIdAsync(int id);
    Task<TResponse> CreateAsync(TCreate dto);
    Task<TResponse?> UpdateAsync(int id, TUpdate dto);
    Task<bool> DeleteAsync(int id);
}

public abstract class CrudService<T, TCreate, TUpdate, TResponse> : ICrudService<T, TCreate, TUpdate, TResponse>
    where T : class
{
    protected readonly IRepository<T> Repository;

    protected CrudService(IRepository<T> repository)
    {
        Repository = repository;
    }

    protected abstract T MapToEntity(TCreate dto);
    protected abstract TResponse MapToResponse(T entity);
    protected abstract void ApplyUpdate(T entity, TUpdate dto);

    public virtual async Task<List<TResponse>> GetAllAsync()
    {
        var items = await Repository.GetAllAsync();
        return items.Select(MapToResponse).ToList();
    }

    public virtual async Task<TResponse?> GetByIdAsync(int id)
    {
        var item = await Repository.GetByIdAsync(id);
        return item != null ? MapToResponse(item) : default;
    }

    public virtual async Task<TResponse> CreateAsync(TCreate dto)
    {
        var entity = MapToEntity(dto);
        entity = await Repository.AddAsync(entity);
        return MapToResponse(entity);
    }

    public virtual async Task<TResponse?> UpdateAsync(int id, TUpdate dto)
    {
        var entity = await Repository.GetByIdAsync(id);
        if (entity == null) return default;

        ApplyUpdate(entity, dto);
        entity = await Repository.UpdateAsync(entity);
        return MapToResponse(entity);
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        return await Repository.DeleteAsync(id);
    }
}
