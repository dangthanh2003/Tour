using System;
using System.Collections.Generic;

namespace QLTours.Models;

public partial class DetailItinerary
{
    public int DetailId { get; set; }

    public int? ItineraryId { get; set; }

    public TimeOnly? ThoiGian { get; set; }

    public string? HoatDong { get; set; }

    public string? MoTa { get; set; }

    public virtual Itinerary? Itinerary { get; set; }
}
