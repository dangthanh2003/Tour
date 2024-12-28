using QLTours.Models;

namespace QLTours.Services;
public interface IVnPayService
{
    string CreatePaymentUrl(Booking newBooking, HttpContext context);
    PaymentResponseModel PaymentExecute(IQueryCollection collections);
}