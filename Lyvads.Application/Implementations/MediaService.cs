
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static PayStack.Net.CustomerList;

namespace Lyvads.Application.Implementations;

public class MediaService : IMediaService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<MediaService> _logger;
    
    public MediaService(
        IOptions<CloudinarySettings> cloudinarySettings,
        ILogger<MediaService> logger)
    {
        var account = new Account(
            cloudinarySettings.Value.CloudName,
            cloudinarySettings.Value.ApiKey,
            cloudinarySettings.Value.ApiSecret);

        _cloudinary = new Cloudinary(account)
        {
            Api = { Secure = true } // Ensures HTTPS URLs
        };
        _logger = logger;
    }


    public async Task<Dictionary<string, string>> UploadVideoAsync(IFormFile video, string folderName)
    {
        var response = new Dictionary<string, string>();
        var defaultSize = 10000000; // Example size limit for videos
        var allowedTypes = new List<string>() { "mp4", "avi", "mov" };

        if (video == null)
        {
            response.Add("Code", "400");
            response.Add("Message", "No file uploaded");
            return response;
        }

        if (video.Length < 1 || video.Length > defaultSize)
        {
            response.Add("Code", "400");
            response.Add("Message", "Invalid size");
            return response;
        }

        var extension = Path.GetExtension(video.FileName).ToLower().TrimStart('.');
        if (!allowedTypes.Contains(extension))
        {
            response.Add("Code", "400");
            response.Add("Message", "Invalid type");
            return response;
        }

        var uploadResult = new VideoUploadResult();

        using (var stream = video.OpenReadStream())
        {
            var uploadParams = new VideoUploadParams()
            {
                File = new FileDescription(video.FileName, stream),
                Transformation = new Transformation()
                    .Width(1920).Height(1080).Crop("scale") // Adjust dimensions
                    .Quality("auto") // Optimize quality
                    .FetchFormat("auto") // Automatically determine best format
                    .Dpr("auto"), // Set dynamic pixel ratio
                Folder = folderName
            };


            uploadResult = await _cloudinary.UploadAsync(uploadParams);
        }

        if (!string.IsNullOrEmpty(uploadResult.PublicId))
        {
            response.Add("Code", "200");
            response.Add("Message", "Upload successful");
            response.Add("PublicId", uploadResult.PublicId);
            response.Add("Url", uploadResult.SecureUrl.ToString());
            return response;
        }

        response.Add("Code", "400");
        response.Add("Message", "Failed to upload");
        return response;
    }


    public async Task<Dictionary<string, string>> UploadImageAsync(IFormFile photo, string folderName)
    {
        var response = new Dictionary<string, string>();
        var defaultSize = 2000000; // Maximum file size: 2MB
        var allowedTypes = new List<string>() { "image/jpeg", "image/jpg", "image/png" }; // Correct MIME types

        Console.WriteLine($"Allowed Types: {string.Join(", ", allowedTypes)}");

        // Check if photo is null
        if (photo == null)
        {
            response.Add("Code", "400");
            response.Add("Message", "No file uploaded");
            return response;
        }

        // Check file size
        if (photo.Length < 1 || photo.Length > defaultSize)
        {
            response.Add("Code", "400");
            response.Add("Message", "Invalid size");
            return response;
        }

        // Check file type
        if (!allowedTypes.Contains(photo.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            response.Add("Code", "400");
            response.Add("Message", "Invalid type");
            return response;
        }

        var uploadResult = new ImageUploadResult();

        // Upload the image to Cloudinary
        using (var stream = photo.OpenReadStream())
        {
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(photo.FileName, stream),
                Transformation = new Transformation()
                    .Width(1920).Height(1080).Crop("scale") // Adjust dimensions
                    .Quality("auto") // Optimize quality
                    .FetchFormat("auto") // Automatically determine best format
                    .Dpr("auto"), // Set dynamic pixel ratio
                Folder = folderName
            };

            uploadResult = await _cloudinary.UploadAsync(uploadParams);
        }

        // Check if the upload was successful
        if (!string.IsNullOrEmpty(uploadResult.PublicId))
        {
            //// Generate the URL using PublicId
            //var imageUrl = _cloudinary.Api.UrlImgUp
            //    .Secure(true) // Use HTTPS
            //    .uploadResult.PublicId
            //    .Transformation(new Transformation()
            //        .Width(1920).Height(1080).Crop("scale")
            //        .Quality("auto")
            //        .FetchFormat("auto")
            //        .Dpr("auto"))
            //    .BuildUrl();

            response.Add("Code", "200");
            response.Add("Message", "Upload successful");
            response.Add("PublicId", uploadResult.PublicId); // Use uploadResult.PublicId
            response.Add("Url", uploadResult.SecureUrl.ToString());

            return response;
        }

        // Handle upload failure
        response.Add("Code", "400");
        response.Add("Message", "Failed to upload");
        return response;
    }


    public async Task<bool> DeleteMediaAsync(string mediaUrl)
    {
        try
        {
            // Check if the media URL is valid
            if (string.IsNullOrEmpty(mediaUrl))
            {
                return false;
            }

            // Simulate the deletion of a file (e.g., from cloud storage)
            // This could be an API call to services like AWS S3, Azure Blob, or a file system operation.
            bool isDeleted = await DeleteFileAsync(mediaUrl);

            return isDeleted;
        }
        catch (Exception ex)
        {
            // Log the exception for debugging purposes
            _logger.LogError(ex, $"Error occurred while deleting media at URL: {mediaUrl}");
            return false;
        }
    }

    public async Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            // Simulate a file deletion (adjust logic for your cloud provider)
            Console.WriteLine($"Deleting file from storage: {fileUrl}");
            await Task.Delay(100); // Simulate async API call
            return true; // Return true if deletion is successful
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while deleting file: {ex.Message}");
            return false;
        }
    }



    public async Task<DeletionResult> DeleteImageAsync(string publicUrl)
    {
        var publicId = publicUrl.Split('/').Last().Split('.')[0];
        var deleteParams = new DeletionParams(publicId);
        return await _cloudinary.DestroyAsync(deleteParams);
    }


    public async Task<DeletionResult> DeleteVideoAsync(string publicUrl)
    {
        var publicId = publicUrl.Split('/').Last().Split('.')[0];
        var deleteParams = new DeletionParams(publicId);

        // Assuming your cloud provider uses the same API for deleting videos
        return await _cloudinary.DestroyAsync(deleteParams);
    }



   
    public Task<Dictionary<string, string>> GetDownloadUrlAsync(string publicId, string folderName)
    {
        var response = new Dictionary<string, string>();

        // Construct the Cloudinary URL for the video
        var downloadUrl = _cloudinary.Api.UrlVideoUp
                            .Action("download")
                            .ResourceType("video")
                            .Secure(true)
                            .Transform(new Transformation().Quality("auto"))
                            .BuildUrl($"{folderName}/{publicId}.mp4"); // Assuming the file is in mp4 format

        if (!string.IsNullOrEmpty(downloadUrl))
        {
            response.Add("Code", "200");
            response.Add("Message", "Download URL generated successfully");
            response.Add("DownloadUrl", downloadUrl);
        }
        else
        {
            response.Add("Code", "400");
            response.Add("Message", "Failed to generate download URL");
        }

        // Return the dictionary wrapped in a completed Task
        return Task.FromResult(response);
    }



    //public async Task<Dictionary<string, string>> UploadImageAsync(IFormFile photo, string folderName)
    //{
    //    var response = new Dictionary<string, string>();
    //    var defaultSize = 2000000;
    //    var allowedTypes = new List<string>() { "jpeg", "jpg", "png" };
    //    Console.WriteLine($"Allowed Types: {string.Join(", ", allowedTypes)}");


    //    if (photo == null)
    //    {
    //        response.Add("Code", "400");
    //        response.Add("Message", "No file uploaded");
    //        return response;
    //    }

    //    var file = photo;

    //    if (file.Length < 1 || file.Length > defaultSize)
    //    {
    //        response.Add("Code", "400");
    //        response.Add("Message", "Invalid size");
    //        return response;
    //    }

    //    if (allowedTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
    //    {
    //        response.Add("Code", "400");
    //        response.Add("Message", "Invalid type");
    //        return response;
    //    }

    //    var uploadResult = new ImageUploadResult();

    //    using (var stream = file.OpenReadStream())
    //    {
    //        var uploadParams = new ImageUploadParams()
    //        {
    //            File = new FileDescription(file.Name, stream),
    //            Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face"),
    //            Folder = folderName
    //        };
    //        uploadResult = await _cloudinary.UploadAsync(uploadParams);
    //    }

    //    if (!string.IsNullOrEmpty(uploadResult.PublicId))
    //    {
    //        response.Add("Code", "200");
    //        response.Add("Message", "Upload successful");
    //        response.Add("PublicId", uploadResult.PublicId);
    //        response.Add("Url", uploadResult.SecureUrl.ToString()); // Use SecureUrl for HTTPS

    //        return response;
    //    }

    //    response.Add("Code", "400");
    //    response.Add("Message", "Failed to upload");
    //    return response;
    //}

    //public async Task<Dictionary<string, string>> UploadVideoAsync(IFormFile video, string folderName)
    //{
    //    var response = new Dictionary<string, string>();
    //    var defaultSize = 10000000; // Example size limit for videos
    //    var allowedTypes = new List<string>() { "mp4", "avi", "mov" }; // Example allowed video types
    //    Console.WriteLine($"Allowed Types: {string.Join(", ", allowedTypes)}");

    //    if (video == null)
    //    {
    //        response.Add("Code", "400");
    //        response.Add("Message", "No file uploaded");
    //        return response;
    //    }

    //    var file = video;

    //    if (file.Length < 1 || file.Length > defaultSize)
    //    {
    //        response.Add("Code", "400");
    //        response.Add("Message", "Invalid size");
    //        return response;
    //    }

    //    var extension = Path.GetExtension(file.FileName).ToLower().TrimStart('.');
    //    if (!allowedTypes.Contains(extension) && !allowedTypes.Contains(file.ContentType))
    //    {
    //        response.Add("Code", "400");
    //        response.Add("Message", "Invalid type");
    //        return response;
    //    }


    //    var uploadResult = new VideoUploadResult(); // Assuming a separate result class for videos

    //    using (var stream = file.OpenReadStream())
    //    {
    //        var uploadParams = new VideoUploadParams()
    //        {
    //            File = new FileDescription(file.Name, stream),
    //            Folder = folderName
    //            // Additional video-specific parameters can be added here
    //        };
    //        uploadResult = await _cloudinary.UploadAsync(uploadParams); // Assuming cloudinary handles video uploads similarly
    //    }

    //    if (!string.IsNullOrEmpty(uploadResult.PublicId))
    //    {
    //        response.Add("Code", "200");
    //        response.Add("Message", "Upload successful");
    //        response.Add("PublicId", uploadResult.PublicId);
    //        response.Add("Url", uploadResult.SecureUrl.ToString()); // Use SecureUrl for HTTPS

    //        return response;
    //    }

    //    response.Add("Code", "400");
    //    response.Add("Message", "Failed to upload");
    //    return response;
    //}


}

