using Microsoft.EntityFrameworkCore;
using ShopService.Core.Entities;
using ShopService.Infrastructure.Context;
using ShopService.Infrastructure.Interfaces.Base;

namespace ShopService.Infrastructure.Repositories.Base;

public class Repository<T> : IRepository<T> where T : class, IEntity
{
    private readonly ShopDbContext _context;
    protected readonly DbSet<T> Entities;
    private IRepository<T> _repositoryImplementation;

    public Repository(ShopDbContext context)
    {
        _context = context;
        Entities = _context.Set<T>();
    }
    
    public virtual IQueryable<T> GetAll() => 
        Entities.AsNoTracking();
    
    public virtual async Task<IEnumerable<T>> GetAllAsync() => 
        await GetAll().ToListAsync();
    
    public virtual async Task<T?> GetByIdAsync(Guid id) =>
        await GetAll().SingleOrDefaultAsync(s => s.Id == id);

    public virtual async Task<T> InsertAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        
        var entityItem = await Entities.AddAsync(entity);
        
        await _context.SaveChangesAsync();
        
        return (await Entities.SingleOrDefaultAsync(s => s.Id == entityItem.Entity.Id))!;
    }

    public async Task InsertManyAsync(IEnumerable<T> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        await Entities.AddRangeAsync(entities);

        await _context.SaveChangesAsync();
    }

    public void Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        Entities.Update(entity);
    }

    public void Delete(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        Entities.Remove(entity);
    }

    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}