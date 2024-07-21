using Lyvads.Infrastructure.Persistence;
using Lyvads.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Lyvads.Infrastructure.Repositories;

public class Repository : IRepository
{
    private readonly AppDbContext _context;

    public Repository(AppDbContext context)
    {
        _context = context;
    }

    public async Task Add<TEntity>(TEntity entity) where TEntity : class
    {
        await _context.Set<TEntity>().AddAsync(entity);
    }

    public IQueryable<TEntity> GetAll<TEntity>() where TEntity : class
    {
        return _context.Set<TEntity>();
    }

    public async Task<TEntity?> GetById<TEntity>(string id) where TEntity : class
    {
        return await _context.Set<TEntity>().FindAsync(id) ?? null;
    }

    public void Remove<TEntity>(TEntity entity) where TEntity : class
    {
        _context.Set<TEntity>().Remove(entity);
    }

    public void Update<TEntity>(TEntity entity) where TEntity : class
    {
        _context.Set<TEntity>().Update(entity);
    }

    public async Task<T?> FindByCondition<T>(Expression<Func<T, bool>> expression) where T : class
    {
        return await _context.Set<T>().FirstOrDefaultAsync(expression);
    }


}
