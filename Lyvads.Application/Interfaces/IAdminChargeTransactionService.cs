

using Lyvads.Application.Dtos;
using Lyvads.Domain.Responses;

namespace Lyvads.Application.Interfaces;

public interface IAdminChargeTransactionService
{
    Task<ServerResponse<ChargeSummaryDto>> GetChargeSummaryAsync();
    Task<ServerResponse<object>> GetChargeSummaryFormatAsync();
    Task<ServerResponse<List<ChargeTransactionDto>>> GetAllChargeTransactionsAsync();
    Task<ServerResponse<CreateChargeResponse>> AddNewChargeAsync(CreateChargeDto chargeDto);
    Task<ServerResponse<EditChargeResponse>> EditChargeAsync(string chargeId, EditChargeDto chargeDto);
    Task<ServerResponse<ChargeTransactionDto>> GetChargeTransactionByIdAsync(string chargeTransactionId);
    Task<ServerResponse<string>> DeleteChargeAsync(string chargeId);
    Task<ServerResponse<List<ChargeDto>>> GetAllChargesAsync();
    Task<ServerResponse<ChargeDto>> GetChargeByIdAsync(string chargeId);
}
