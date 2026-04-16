namespace HotelBooking.API.Services;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string email, string name);
    Task SendVerificationEmailAsync(string email, string name, string token);
    Task SendBookingConfirmationEmailAsync(string email, string name, string hotelName,
        DateTime checkIn, DateTime checkOut, decimal amount, int bookingId, int qty);
    Task SendCancellationEmailAsync(string email, string name, string hotelName, int bookingId);
}