using System;
using System.Collections.Generic;

namespace QLTours.Models;

public partial class ItineraryImage
{
    public int ImageId { get; set; }

    public int ItineraryId { get; set; }

    public string ImagePath { get; set; } = null!;

    public virtual Itinerary Itinerary { get; set; } = null!;
}
