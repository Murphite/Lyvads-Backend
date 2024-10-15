using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lyvads.Application.Dtos.CreatorDtos;

public class CommentResponseDto
{
    public string? CommentId { get; set; }
    public string? ParentCommentId { get; set; }
    public string? PostId { get; set; }
    public string? Content { get; set; }
    public string? UserId { get; set; }
    public string? CommentBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
