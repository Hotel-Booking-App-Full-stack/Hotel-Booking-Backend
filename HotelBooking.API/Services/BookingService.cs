using HotelBooking.API.Data;
using HotelBooking.API.DTOs;
using HotelBooking.API.Models;
using HotelBooking.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.API.Services;

public class BookingService : IBookingService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<BookingService> _logger;

    public BookingService(AppDbContext context, IEmailService emailService, ILogger<BookingService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<BookingDto> CreateAsync(int userId, CreateBookingDto dto)
    {
        var room = await _context.Rooms.Include(r => r.Hotel).FirstOrDefaultAsync(r => r.Id == dto.RoomId)
            ?? throw new InvalidOperationException("Room not found.");

        if (!room.IsAvailable)
            throw new InvalidOperationException("Room is not available.");

        var nights = (dto.CheckOutDate - dto.CheckInDate).Days;
        if (nights <= 0)
            throw new InvalidOperationException("Check-out must be after check-in.");

        var booking = new Booking
        {
            UserId = userId,
            RoomId = dto.RoomId,
            HotelId = dto.HotelId,
            CheckInDate = dto.CheckInDate,
            CheckOutDate = dto.CheckOutDate,
            TotalAmount = room.PricePerNight * nights,
            SpecialRequests = dto.SpecialRequests,
            Status = "Confirmed"
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId);
        if (user != null)
            await _emailService.SendBookingConfirmationEmailAsync(
                user.Email, user.FullName, room.Hotel.Name,
                booking.CheckInDate, booking.CheckOutDate, booking.TotalAmount, booking.Id);

        _logger.LogInformation("Booking created: #{Id} by user {UserId}", booking.Id, userId);

        return (await GetByIdAsync(booking.Id))!;
    }

    public async Task<List<BookingDto>> GetUserBookingsAsync(int userId)
    {
        return await _context.Bookings
            .Include(b => b.User).Include(b => b.Room).Include(b => b.Hotel)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => MapToDto(b))
            .ToListAsync();
    }

    public async Task<List<BookingDto>> GetAllBookingsAsync()
    {
        return await _context.Bookings
            .Include(b => b.User).Include(b => b.Room).Include(b => b.Hotel)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => MapToDto(b))
            .ToListAsync();
    }

    public async Task<BookingDto?> GetByIdAsync(int id)
    {
        var b = await _context.Bookings
            .Include(b => b.User).Include(b => b.Room).Include(b => b.Hotel)
            .FirstOrDefaultAsync(b => b.Id == id);
        return b == null ? null : MapToDto(b);
    }

    public async Task<bool> CancelAsync(int id, int userId, string role)
    {
        var booking = await _context.Bookings
            .Include(b => b.User).Include(b => b.Hotel)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null) return false;
        if (role != "Admin" && booking.UserId != userId)
            throw new UnauthorizedAccessException("Cannot cancel another user's booking.");

        booking.Status = "Cancelled";
        booking.CancelledAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _emailService.SendCancellationEmailAsync(
            booking.User.Email, booking.User.FullName, booking.Hotel.Name, booking.Id);

        return true;
    }

    private static BookingDto MapToDto(Booking b) => new()
    {
        Id = b.Id,
        UserId = b.UserId,
        UserName = b.User?.FullName ?? "",
        UserEmail = b.User?.Email ?? "",
        RoomId = b.RoomId,
        RoomNumber = b.Room?.RoomNumber ?? "",
        RoomType = b.Room?.RoomType ?? "",
        HotelId = b.HotelId,
        HotelName = b.Hotel?.Name ?? "",
        HotelLocation = b.Hotel?.Location ?? "",
        CheckInDate = b.CheckInDate,
        CheckOutDate = b.CheckOutDate,
        TotalAmount = b.TotalAmount,
        Status = b.Status,
        SpecialRequests = b.SpecialRequests,
        CreatedAt = b.CreatedAt,
        CancelledAt = b.CancelledAt
    };
}

