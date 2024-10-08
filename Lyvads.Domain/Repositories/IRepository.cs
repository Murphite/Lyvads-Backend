

using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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
}
