
using Lyvads.Application.Interfaces;

namespace Lyvads.Application.Implementations;

public class EmailContext : IEmailContext
{
    public string VerifiedEmail { get; set; } = string.Empty;
}
