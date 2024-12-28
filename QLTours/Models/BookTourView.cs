namespace QLTours.Models
{
    public class BookTourViewModel
    {
        public int TourId { get; set; }
        public int Quantity { get; set; }
        public string DiscountCode { get; set; }
        public string TourName { get; set; } = null!;
        public string? Img { get; set; }
        public decimal Price { get; set; }

    }
}
