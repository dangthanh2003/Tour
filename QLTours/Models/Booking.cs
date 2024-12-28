using System;
using System.Collections.Generic;

namespace QLTours.Models;

public partial class Booking
{
    public int BookingId { get; set; }

    public int? UserId { get; set; }

    public int? TourId { get; set; }

    public decimal Total { get; set; }

    public DateTime BookingDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual Tour? Tour { get; set; }

    public virtual User? User { get; set; }
}
