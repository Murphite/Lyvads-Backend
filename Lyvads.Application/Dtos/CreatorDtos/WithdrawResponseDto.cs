

namespace Lyvads.Application.Dtos.CreatorDtos;

public class WithdrawResponseDto
{
    public string CreatorId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public decimal RemainingBalance { get; set; }
    public DateTimeOffset TransactionDate { get; set; }
}
