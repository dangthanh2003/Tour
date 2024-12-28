using System;
using System.Collections.Generic;

namespace QLTours.Models;

public partial class Vehicle
{
    public int VehicleId { get; set; }

    public string VehicleName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual ICollection<TourDetail> TourDetails { get; set; } = new List<TourDetail>();
}
