using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lyvads.Application.Dtos.CreatorDtos;

public class CreatorDto
{
    public string FullName { get; set; } = default!;
    public string Industry { get; set; } = default!;
    public decimal AdvertAmount { get; set; }
}


public class FilterCreatorDto
{
    public string CreatorId { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Industry { get; set; } = default!;
    public string Location { get; set; } = default!;
    public decimal Price { get; set; }
}
