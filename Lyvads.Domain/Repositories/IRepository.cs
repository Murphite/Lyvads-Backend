

using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Lyvads.Domain.Entities;

namespace Lyvads.Domain.Repositories;

public interface IRepository
{
    public Task Add<TEntity>(TEntity entity) where TEntity : class;
    public IQueryable<TEntity> GetAll<TEntity>() where TEntity : class;
    public Task<TEntity> GetById<TEntity>(string id) where TEntity : class;
    public void Update<TEntity>(TEntity entity) where TEntity : class;
    public void Remove<TEntity>(TEntity entity) where TEntity : class;
    public Task<T?> FindByCondition<T>(Expression<Func<T, bool>> expression) where T : class;

    Task<IDbContextTransaction> BeginTransactionAsync();
    Task<Comment?> GetCommentByIdAsync(string commentId);

    Task<bool> Exists<T>(Expression<Func<T, bool>> predicate) where T : class;
    Task<IEnumerable<T>> FindAllByCondition<T>(Expression<Func<T, bool>> predicate) where T : class;
    IQueryable<T> QueryFindByCondition<T>(Expression<Func<T, bool>> expression) where T : class;
}
