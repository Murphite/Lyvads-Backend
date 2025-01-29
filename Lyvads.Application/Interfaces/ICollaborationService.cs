
using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Domain.Enums;
using Lyvads.Domain.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lyvads.Application.Interfaces;

public interface ICollaborationService
{
    Task<ServerResponse<List<CollaborationDto>>> GetCollaborationsAsync();
    Task<ServerResponse<CollaborationDetailsDto>> GetCollaborationDetailsAsync(string collaborationId);
    Task<ServerResponse<bool>> FlagToggleCollaborationAsync(string collaborationId);
    Task<ServerResponse<List<GetRequestDto>>> GetAllRequestsByUserAsync(string userId, RequestStatus status);
    Task<ServerResponse<RequestDetailsDto>> GetRequestDetailsAsync(string requestId);
    Task<ServerResponse<DeclineResponseDto>> DeclineRequestAsync(DeclineRequestDto declineRequestDto);
    Task<ServerResponse<DisputeResponseDto>> OpenDisputeAsync(string userId, string requestId, OpenDisputeDto disputeDto);
    Task<ServerResponse<List<GetUserRequestDto>>> GetAllRequestsForCreatorAsync(string creatorId, RequestStatus status);
    Task<ServerResponse<List<FetchDisputeDto>>> FetchDisputesByCreatorAsync(string userId);
    Task<ServerResponse<VideoResponseDto>> SendVideoToUserAsync(string requestId, IFormFile video);
    Task<ServerResponse<DisputeDetailsDto>> GetDisputeDetailsByIdAsync(string disputeId, string userId);
    Task<ServerResponse<ResendResponseDto>> ResendRequestAsync(ResendRequestDto resendRequestDto);
    Task<ServerResponse<CloseRequestResultDto>> CloseRequestAsync(CloseRequestDto closeRequestDto);
    Task<ServerResponse<DeclineDetailsDto>> GetDeclinedDetailsAsync(string requestId);

}
