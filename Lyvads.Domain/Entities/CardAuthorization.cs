


namespace Lyvads.Domain.Entities;

public class CardAuthorization : Entity, IAuditable
{
    public string? AuthorizationCode { get; set; }
    public string? Email { get; set; }
    public string? CardType { get; set; }
    public string? Last4 { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string? Bank { get; set; }
    public string? AccountName { get; set; }
    public bool Reusable { get; set; }
    public string? CountryCode { get; set; }
    public string? Bin { get; set; }
    public string? Signature { get; set; }
    public string? Channel { get; set; }
    public DateTimeOffset CreatedAt { get ; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

