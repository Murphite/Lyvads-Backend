

using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Application.Dtos;
using Lyvads.Domain.Responses;
using System.Threading.Tasks;

namespace Lyvads.Application.Interfaces;

public interface IPromotionService
{
    Task<ServerResponse<PromotionDto>> AddPromotion(CreatePromotionDto createPromotionDto);
    Task<ServerResponse<PromotionDto>> UpdatePromotion(string promotionId, UpdatePromotionDto updatePromotionDto);
    Task<ServerResponse<object>> DeletePromotion(string promotionId);
    Task<ServerResponse<object>> TogglePromotionVisibility(string promotionId);
    Task<ServerResponse<List<PromotionDto>>> GetAllPromotions(bool? isHidden = null);
}
