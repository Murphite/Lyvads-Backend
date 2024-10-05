

using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Domain.Responses;

namespace Lyvads.Application.Interfaces;

public interface IAdminPostService
{
    Task<ServerResponse<List<AdminPostDto>>> GetAllPostsAsync();
    Task<ServerResponse<AdminPostDetailsDto>> GetPostDetailsAsync(string postId);
    Task<ServerResponse<bool>> FlagPostAsync(int postId);
    Task<ServerResponse<bool>> DeletePostAsync(int postId);
}
