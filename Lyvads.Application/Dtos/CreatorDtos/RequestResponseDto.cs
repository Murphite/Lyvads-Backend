using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lyvads.Application.Dtos.CreatorDtos;

public class RequestResponseDto
{
    public string RequestId { get; set; }
    public string Status { get; set; }
    public string UserId { get; set; }
    public string FullName { get; set; }
}
