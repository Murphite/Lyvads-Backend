
namespace Lyvads.Application.Dtos.WalletDtos;

public class BankDto
{

}
public class BankTransferRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string UserId { get; set; }
    public string Reference { get; set; }
    public string BankName { get; set; }
    public string AccountNumber { get; set; }
    public string AccountHolderName { get; set; }
}

public class BankTransferResponse
{
    public bool IsSuccess { get; set; }
    public string TransferInstructions { get; set; } // Instructions or confirmation message
}
