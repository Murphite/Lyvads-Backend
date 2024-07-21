using Lyvads.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lyvads.Application.Dtos.CreatorDtos;

public class PostDto
{
    public string Caption { get; set; }
    public string MediaUrl { get; set; }
    public string Location { get; set; }
    public PostVisibility Visibility { get; set; }
}
