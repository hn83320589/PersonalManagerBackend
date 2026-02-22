using Microsoft.EntityFrameworkCore;
using PersonalManager.Api.Data;

namespace PersonalManager.Api.Repositories;

public class EfRepository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _set;

    public EfRepository(ApplicationDbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }

    public async Task<List<T>> GetAllAsync() =>
        await _set.ToListAsync();

    public async Task<T?> GetByIdAsync(int id) =>
        await _set.FindAsync(id);

    public async Task<List<T>> FindAsync(Func<T, bool> predicate) =>
        await Task.FromResult(_set.AsEnumerable().Where(predicate).ToList());

    public async Task<T> AddAsync(T entity)
    {
        _set.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        _set.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _set.FindAsync(id);
        if (entity is null) return false;
        _set.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
