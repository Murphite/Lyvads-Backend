using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lyvads.Application.Dtos.CreatorDtos;

public class CreatorDto
{
    public string FullName { get; set; } = default!;
    public string Industry { get; set; } = default!;
    public decimal AdvertAmount { get; set; }
}


public class CommentOnPostDto
{
    public string postId { get; set; } = default!;
    public string content { get; set; } = default!;
}


public class FilterCreatorDto
{
    public string ImageUrl { get; set; } = default!;
    public string CreatorId { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Industry { get; set; } = default!;
    public string Location { get; set; } = default!;
    public decimal Price { get; set; }
}


public class PostCommentResponseDto
{
    public string? CommentId { get; set; }
    public string? Content { get; set; }
    public string? UserId { get; set; }
    public string? CommentedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
