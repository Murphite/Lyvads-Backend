
using Lyvads.Domain.Entities;
using Lyvads.Shared.DTOs;

namespace Lyvads.Domain.Repositories;

public interface IPromotionPlanRepository
{
    Task AddAsync(PromotionPlan promotionPlan);
    Task<PromotionPlan?> GetByIdAsync(string planId);
    Task<List<PromotionPlan>> GetAllAsync();
    Task<PaginatorDto<IEnumerable<PromotionPlan>>> GetPaginatedPlansAsync(PaginationFilter paginationFilter);
}
