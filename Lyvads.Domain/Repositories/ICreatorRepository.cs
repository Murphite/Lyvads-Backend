using Lyvads.Domain.Entities;

namespace Lyvads.Domain.Repositories;

public interface ICreatorRepository
{
    Task AddAsync(Creator creator);
    Task<Creator?> GetCreatorByIdAsync(string creatorId);
    IQueryable<Creator> GetCreators();
    Task<Creator?> GetCreatorByApplicationUserIdAsync(string applicationUserId);
}
