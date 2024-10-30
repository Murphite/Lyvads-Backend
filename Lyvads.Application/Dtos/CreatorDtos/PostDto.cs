using Lyvads.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lyvads.Application.Dtos.CreatorDtos;

public class PostDto
{
    public string Caption { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    //public List<IFormFile> MediaUrls { get; set; } = new List<IFormFile>();
    //public PostVisibility Visibility { get; set; }
}

public class UpdatePostDto
{
    //public string? PostId { get; set; }
    public string Caption { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    //public PostVisibility? Visibility { get; set; }
}
