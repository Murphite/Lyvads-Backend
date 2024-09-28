

using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace Lyvads.Application.Interfaces;

public interface IMediaService
{
    Task<DeletionResult> DeleteImageAsync(string publicUrl);
    Task<DeletionResult> DeleteVideoAsync(string publicUrl);
    Task<Dictionary<string, string>> UploadImageAsync(IFormFile photo, string folderName);
    Task<Dictionary<string, string>> UploadVideoAsync(IFormFile video, string folderName);
    Task<Dictionary<string, string>> GetDownloadUrlAsync(string publicId, string folderName);
}
