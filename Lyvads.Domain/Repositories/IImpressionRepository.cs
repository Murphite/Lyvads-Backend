

using Lyvads.Domain.Entities;

namespace Lyvads.Domain.Repositories;

public interface IImpressionRepository
{
    Task<int> CountAsync(); // Count total impressions
    Task<int> CountByUserAsync(string userId); // Count impressions for a specific user
    Task<int> CountByCreatorAsync(string creatorId); // Count impressions for a specific creator
    Task AddAsync(Impression impression); // Add a new impression
}