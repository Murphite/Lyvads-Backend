

using Lyvads.Application.Dtos;
using Lyvads.Domain.Responses;

namespace Lyvads.Application.Interfaces;

public interface IChargeTransactionService
{
    Task<ServerResponse<List<ChargeTransactionDto>>> GetAllChargeTransactionsAsync();
    Task<ServerResponse<string>> AddNewChargeAsync(CreateChargeDto chargeDto);
    Task<ServerResponse<string>> EditChargeAsync(string chargeId, EditChargeDto chargeDto);
    Task<ServerResponse<ChargeTransactionDto>> GetChargeTransactionByIdAsync(string chargeTransactionId);
    Task<ServerResponse<string>> DeleteChargeAsync(string chargeId);
    Task<ServerResponse<List<ChargeDto>>> GetAllChargesAsync();
    Task<ServerResponse<ChargeDto>> GetChargeByIdAsync(string chargeId);
}
