using System;
using System.Collections.Generic;

namespace QLTours.Models;

public partial class Hotel
{
    public int HotelId { get; set; }

    public string HotelName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Address { get; set; } = null!;

    public virtual ICollection<TourDetail> TourDetails { get; set; } = new List<TourDetail>();
}
