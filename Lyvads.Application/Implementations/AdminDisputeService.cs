

using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Repositories;
using Lyvads.Domain.Responses;
using Microsoft.Extensions.Logging;

namespace Lyvads.Application.Implementations;

public class AdminDisputeService : IDisputeService
{
    private readonly ILogger<AdminDisputeService> _logger;
    private readonly IDisputeRepository _disputeRepository;

    public AdminDisputeService(
        ILogger<AdminDisputeService> logger, IDisputeRepository disputeRepository)
    {
        _logger = logger;
        _disputeRepository = disputeRepository;
    }

    public async Task<ServerResponse<List<DisputeDto>>> GetAllDisputesAsync()
    {
        try
        {
            var disputes = await _disputeRepository.GetAllAsync();

            var disputeDtos = disputes.Select(d => new DisputeDto
            {
                Id = d.Id,
                RegularUserName = $"{d.RegularUser.ApplicationUser!.FirstName} {d.RegularUser.ApplicationUser.LastName}",
                RegularUserProfilePic = d.RegularUser.ApplicationUser!.ImageUrl,
                CreatorName = $"{d.Creator.ApplicationUser!.FirstName} {d.Creator.ApplicationUser.LastName}",
                CreatorProfilePic = d.Creator.ApplicationUser!.ImageUrl,
                Amount = d.Amount,
                DisputeDesc = d.DisputeMessage,
                Reason = d.Reason.ToString(),
                FlaggedDate = d.CreatedAt,
                Status = d.Status.ToString()
            }).ToList();

            return new ServerResponse<List<DisputeDto>>
            {
                IsSuccessful = true,
                Data = disputeDtos,
                ResponseMessage = "Disputes retrieved successfully."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving disputes.");
            return new ServerResponse<List<DisputeDto>>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "Internal Server Error",
                    ResponseDescription = ex.Message
                }
            };
        }
    }

    public async Task<ServerResponse<DisputeDto>> GetDisputeDetailsAsync(string disputeId)
    {
        try
        {
            var dispute = await _disputeRepository.GetByIdAsync(disputeId);
            if (dispute == null)
            {
                return new ServerResponse<DisputeDto>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "404",
                        ResponseMessage = "Dispute not found",
                        ResponseDescription = $"Dispute with ID {disputeId} does not exist."
                    }
                };
            }

            var disputeDto = new DisputeDto
            {
                Id = dispute.Id,
                RegularUserName = $"{dispute.RegularUser.ApplicationUser!.FirstName} {dispute.RegularUser.ApplicationUser.LastName}",
                RegularUserProfilePic = dispute.RegularUser.ApplicationUser!.ImageUrl,
                CreatorName = $"{dispute.Creator.ApplicationUser!.FirstName} {dispute.Creator.ApplicationUser.LastName}",
                CreatorProfilePic = dispute.Creator.ApplicationUser!.ImageUrl,
                Amount = dispute.Amount,
                Reason = dispute.Reason.ToString(),
                DisputeDesc = dispute.DisputeMessage,
                FlaggedDate = dispute.CreatedAt,
                Status = dispute.Status.ToString()
            };

            return new ServerResponse<DisputeDto>
            {
                IsSuccessful = true,
                Data = disputeDto,
                ResponseMessage = "Dispute details retrieved successfully."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dispute details for ID {DisputeId}", disputeId);
            return new ServerResponse<DisputeDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "Internal Server Error",
                    ResponseDescription = ex.Message
                }
            };
        }
    }

}
