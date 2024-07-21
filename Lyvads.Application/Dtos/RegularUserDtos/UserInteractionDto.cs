
namespace Lyvads.Application.Dtos.RegularUserDtos;

public class CommentDto
{
    public string UserId { get; set; }
    public string Content { get; set; }
}

public class LikeDto
{
    public string UserId { get; set; }
    public string ContentId { get; set; }
}

public class FundWalletDto
{
    public string UserId { get; set; }
    public decimal Amount { get; set; }
}
