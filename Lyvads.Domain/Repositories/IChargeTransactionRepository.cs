

using Lyvads.Domain.Entities;

namespace Lyvads.Domain.Repositories;

public interface IChargeTransactionRepository
{
    Task<List<ChargeTransaction>> GetAllAsync();
    Task<ChargeTransaction> GetByCTIdAsync(string chargeId);
    Task AddAsync(Charge charge);
    Task UpdateAsync(Charge charge);
    Task DeleteAsync(Charge charge);
    Task<List<Charge>> GetAllChargesAsync();
    Task<Charge> GetChargeByIdAsync(string chargeId);
}
