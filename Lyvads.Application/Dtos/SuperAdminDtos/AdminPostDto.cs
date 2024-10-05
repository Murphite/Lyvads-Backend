

using Lyvads.Domain.Enums;

namespace Lyvads.Application.Dtos.SuperAdminDtos;

public class AdminPostDto
{
    public string? PostId { get; set; }
    public string? CreatorName { get; set; }
    public string? Caption { get; set; }
    public DateTimeOffset DatePosted { get; set; }
    public PostStatus Status { get; set; }
}

public class AdminPostDetailsDto
{
    public string? PostId { get; set; }
    public string? CreatorName { get; set; }
    public string? Caption { get; set; }
    public DateTimeOffset DatePosted { get; set; }
    public PostStatus Status { get; set; }
    public List<AdminCommentDto>? Comments { get; set; }
    public List<AdminLikeDto>? Likes { get; set; }
}

public class AdminCommentDto
{
    public string? UserName { get; set; }
    public string? Text { get; set; }
    public DateTimeOffset DateCommented { get; set; }
}


public class AdminLikeDto
{
    public string? UserName { get; set; }
}



