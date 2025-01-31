

using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Application.Dtos;
using Lyvads.Domain.Responses;
using System.Threading.Tasks;
using Lyvads.Domain.Entities;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Shared.DTOs;

namespace Lyvads.Application.Interfaces;

public interface IPromotionService
{
    Task<ServerResponse<PromotionDto>> AddPromotion(CreatePromotionDto createPromotionDto);
    Task<ServerResponse<PromotionDto>> UpdatePromotion(string promotionId, UpdatePromotionDto updatePromotionDto);
    Task<ServerResponse<object>> DeletePromotion(string promotionId);
    Task<ServerResponse<object>> TogglePromotionVisibility(string promotionId);
    Task<ServerResponse<List<PromotionDto>>> GetAllPromotions(bool? isHidden = null);

   
}

public interface IPromotionPlanService
{
    Task<ServerResponse<PromotionPlan>> CreatePromotionPlanAsync(CreatePromotionPlanDto planDto);
   
    Task<ServerResponse<SubscriptionPaymentResponseDto>> SubscribeToPromotionPlanAsync(string planId, string creatorId);
    
    Task<ServerResponse<PaginatorDto<IEnumerable<PromotionPlan>>>> GetAvailablePromotionPlansAsync(PaginationFilter paginationFilter);
    Task<ServerResponse<PaginatorDto<IEnumerable<SubscribedCreatorDto>>>> GetSubscribedCreatorsAsync(PaginationFilter paginationFilter);
}