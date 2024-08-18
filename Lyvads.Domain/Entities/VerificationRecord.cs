using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lyvads.Domain.Entities;

public class VerificationRecord
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Code { get; set; }
    public bool IsVerified { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }
}
