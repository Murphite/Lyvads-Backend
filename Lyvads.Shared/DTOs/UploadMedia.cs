
using Microsoft.AspNetCore.Http;

namespace Lyvads.Shared.DTOs;

public class UploadImage
{
    public IFormFile? Image { get; set; }
}

public class UploadVideo
{
    public IFormFile? Video { get; set; }
}
