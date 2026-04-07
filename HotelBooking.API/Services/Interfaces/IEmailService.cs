namespace HotelBooking.API.Services;

public interface IEmailService
{
    Task SendRegistrationEmailAsync(string email, string name);
    Task SendBookingConfirmationEmailAsync(string email, string name, string hotelName, DateTime checkIn, DateTime checkOut, decimal amount, int bookingId);
    Task SendCancellationEmailAsync(string email, string name, string hotelName, int bookingId);
}