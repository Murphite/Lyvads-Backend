using Lyvads.Domain.Entities;


namespace Lyvads.Domain.Repositories;

public interface IUserAdRepository
{
    Task<List<UserAd>> GetAllAsync();
    Task<UserAd> GetByIdAsync(string adId);
    Task UpdateAsync(UserAd ad);
    Task AddUserAdAsync(UserAd userAd);
    Task<UserAd?> GetUserAdByIdAsync(string adId);
    Task UpdateUserAdAsync(UserAd userAd);
}