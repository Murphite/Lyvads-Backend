using Lyvads.Infrastructure.Persistence;
using Lyvads.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using Lyvads.Domain.Entities;
using Lyvads.Shared.DTOs;

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

    public async Task<TEntity> GetById<TEntity>(string id) where TEntity : class
    {
        var entity = await _context.Set<TEntity>().FindAsync(id);
        return entity ?? throw new InvalidOperationException("Entity not found.");
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

    public IQueryable<T> QueryFindByCondition<T>(Expression<Func<T, bool>> expression) where T : class
    {
        return _context.Set<T>().Where(expression);
    }


    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }

    public async Task<Comment?> GetCommentByIdAsync(string commentId)
    {
        return await _context.Comments
            .Include(c => c.Replies)
            .FirstOrDefaultAsync(c => c.Id == commentId);
    }

    public async Task<bool> Exists<T>(Expression<Func<T, bool>> predicate) where T : class
    {
        return await _context.Set<T>().AnyAsync(predicate);
    }

    public async Task<IEnumerable<T>> FindAllByCondition<T>(Expression<Func<T, bool>> predicate) where T : class
    {
        return await _context.Set<T>().Where(predicate).ToListAsync();
    }

    public async Task<int> CountByCondition<T>(Expression<Func<T, bool>> expression) where T : class
    {
        return await _context.Set<T>().Where(expression).CountAsync();
    }

    public async Task<IEnumerable<T>> FindPaginatedByCondition<T>(Expression<Func<T, bool>> expression, PaginationFilter paginationFilter) where T : class
    {
        return await _context.Set<T>()
                 .Where(expression)
                 .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize) // Skip items for the current page
                 .Take(paginationFilter.PageSize) // Take the page size amount of items
                 .ToListAsync();
    }
}
