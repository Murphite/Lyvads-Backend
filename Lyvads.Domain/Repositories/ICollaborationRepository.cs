

using Lyvads.Domain.Entities;

namespace Lyvads.Domain.Repositories;

public interface ICollaborationRepository
{
    IQueryable<Request> GetAllCollaborations();
    Task<Request?> GetByIdAsync(string id);
    Task<List<Request>> GetAllAsync();
    Task UpdateAsync(Request entity);
}
