using System;
using System.Collections.Generic;

namespace QLTours.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
}
