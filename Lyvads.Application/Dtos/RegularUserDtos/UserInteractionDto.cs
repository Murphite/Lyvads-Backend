
namespace Lyvads.Application.Dtos.RegularUserDtos;

public class CommentDto
{
    public string UserId { get; set; } = default!;
    public string Content { get; set; } = default!;
}

public class LikeDto
{
    public string UserId { get; set; } = default!;
    public string ContentId { get; set; } = default!;
}

public class FundWalletDto
{
    public string UserId { get; set; } = default!;
    public decimal Amount { get; set; } = default!;
}
