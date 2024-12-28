using System;
using System.Collections.Generic;

namespace QLTours.Models;

public partial class TourDetail
{
    public int TourDetailId { get; set; }

    public int? TourId { get; set; }

    public int? VehicleId { get; set; }

    public int? HotelId { get; set; }

    public virtual Hotel? Hotel { get; set; }

    public virtual Tour? Tour { get; set; }

    public virtual Vehicle? Vehicle { get; set; }
}
