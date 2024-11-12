

using Lyvads.Domain.Entities;

namespace Lyvads.Domain.Repositories;

public interface IPromotionRepository
{
    Task AddAsync(Promotion promotion);
    Task UpdateAsync(Promotion promotion);
    Task DeleteAsync(Promotion promotion);
    Task<Promotion> GetByIdAsync(string id);
    Task<List<Promotion>> GetAllAsync();
}
