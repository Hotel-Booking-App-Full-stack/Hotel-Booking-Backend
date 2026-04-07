using HotelBooking.API.DTOs;

namespace HotelBooking.API.Services;

public interface IBookingService
{
    Task<BookingDto> CreateAsync(int userId, CreateBookingDto dto);
    Task<List<BookingDto>> GetUserBookingsAsync(int userId);
    Task<List<BookingDto>> GetAllBookingsAsync();
    Task<BookingDto?> GetByIdAsync(int id);
    Task<bool> CancelAsync(int id, int userId, string role);
}