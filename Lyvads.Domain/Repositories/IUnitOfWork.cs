
namespace Lyvads.Domain.Repositories;

public interface IUnitOfWork
{
    public Task<int> SaveChangesAsync();
}
