

using Lyvads.Domain.Entities;

namespace Lyvads.Domain.Repositories;

public interface ICollaborationRepository
{
    IQueryable<Collaboration> GetAllCollaborations();
    Task<Collaboration?> GetByIdAsync(string id);
    Task<List<Collaboration>> GetAllAsync();
}
