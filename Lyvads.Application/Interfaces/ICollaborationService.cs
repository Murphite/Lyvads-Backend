
using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Domain.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lyvads.Application.Interfaces;

public interface ICollaborationService
{
    Task<ServerResponse<List<CollaborationDto>>> GetCollaborationsAsync();
    Task<ServerResponse<CollaborationDto>> GetCollaborationDetailsAsync(string collaborationId);
    Task<ServerResponse<FileStreamResult>> DownloadReceiptAsync(string collaborationId);
    Task<ServerResponse<List<GetRequestDto>>> GetAllRequestsByUserAsync(string userId, string status);
    Task<ServerResponse<RequestDetailsDto>> GetRequestDetailsAsync(string requestId);
    //Task<ServerResponse<bool>> DownloadVideoAsync(string requestId, bool withWatermark);
    //Task<ServerResponse<bool>> OpenDisputeAsync(string requestId, string reason);
    Task<ServerResponse<DisputeResponseDto>> OpenDisputeAsync(string requestId, OpenDisputeDto disputeDto);
    Task<ServerResponse<List<GetUserRequestDto>>> GetAllRequestsForCreatorAsync(string creatorId, string status);
    Task<ServerResponse<List<FetchDisputeDto>>> FetchDisputesByCreatorAsync(string userId);
    Task<ServerResponse<VideoResponseDto>> SendVideoToUserAsync(string requestId, IFormFile video);
    Task<ServerResponse<DisputeDetailsDto>> GetDisputeDetailsByIdAsync(string disputeId, string userId);



}
