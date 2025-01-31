

using Lyvads.Domain.Entities;
using Lyvads.Shared.DTOs;

namespace Lyvads.Domain.Repositories;

public interface IPromotionSubRepository
{
    Task<PromotionSubscription> GetByPaymentReferenceAsync(string paymentReference);
    Task UpdateAsync(PromotionSubscription subscription);
    Task AddAsync(PromotionSubscription subscription);
    Task<List<PromotionSubscription>> GetAllSubscriptionsAsync();
    Task<Creator?> GetCreatorByIdAsync(string creatorId);
    Task<PaginatorDto<IEnumerable<PromotionSubscription>>> GetPaginatedSubscriptionsAsync(PaginationFilter paginationFilter);
}
