

using Lyvads.Domain.Entities;

namespace Lyvads.Domain.Repositories;

public interface IChargeTransactionRepository
{
    Task<List<ChargeTransaction>> GetAllAsync();
    Task<ChargeTransaction> GetByCTIdAsync(string chargeId);
    Task<List<Charge>> GetChargeDetailsAsync();
    Task AddAsync(Charge charge);
    Task UpdateAsync(Charge charge);
    Task DeleteAsync(Charge charge);
    Task<List<Charge>> GetAllChargesAsync();
    Task<Charge> GetChargeByIdAsync(string chargeId);
    Task<ApplicationUser> GetApplicationUserWithRoles(string id);
    Task AddRangeAsync(IEnumerable<ChargeTransaction> chargeTransactions);
}
