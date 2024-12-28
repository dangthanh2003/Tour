using System;
using System.Collections.Generic;

namespace QLTours.Models;

public partial class Itinerary
{
    public int ItineraryId { get; set; }

    public int? TourId { get; set; }

    public DateOnly? Ngay { get; set; }

    public string? ChiTietLichTrinh { get; set; }

    public virtual ICollection<DetailItinerary> DetailItineraries { get; set; } = new List<DetailItinerary>();

    public virtual ICollection<ItineraryImage> ItineraryImages { get; set; } = new List<ItineraryImage>();

    public virtual Tour? Tour { get; set; }
}
