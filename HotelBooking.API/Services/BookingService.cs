using HotelBooking.API.Data;
using HotelBooking.API.DTOs;
using HotelBooking.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.API.Services;

public class BookingService : IBookingService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _email;
    private readonly ILogger<BookingService> _logger;

    public BookingService(AppDbContext c, IEmailService e, ILogger<BookingService> l)
    { _context = c; _email = e; _logger = l; }

    public async Task<BookingDto> CreateAsync(int userId, CreateBookingDto dto)
    {
        var room = await _context.Rooms
            .Include(r => r.Hotel).Include(r => r.Bookings)
            .FirstOrDefaultAsync(r => r.Id == dto.RoomId)
            ?? throw new InvalidOperationException("Room not found.");

        var nights = (dto.CheckOutDate - dto.CheckInDate).Days;
        if (nights <= 0) throw new InvalidOperationException("Check-out must be after check-in.");

        var currentBooked = room.Bookings.Count(b => b.Status == "Confirmed");
        var qty = Math.Max(1, dto.Quantity);
        if (currentBooked + qty > room.TotalRooms)
            throw new InvalidOperationException(
                $"Only {room.TotalRooms - currentBooked} rooms available.");

        var booking = new Booking
        {
            UserId = userId,
            RoomId = dto.RoomId,
            HotelId = dto.HotelId,
            CheckInDate = dto.CheckInDate,
            CheckOutDate = dto.CheckOutDate,
            TotalAmount = room.PricePerNight * nights * qty,
            Quantity = qty,
            SpecialRequests = dto.SpecialRequests,
            Status = "Confirmed"
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Booking #{Id} created: {Qty}x {Type}", booking.Id, qty, room.RoomType);

        var user = await _context.Users.FindAsync(userId);
        if (user != null)
            await _email.SendBookingConfirmationEmailAsync(user.Email, user.FullName,
                room.Hotel.Name, booking.CheckInDate, booking.CheckOutDate,
                booking.TotalAmount, booking.Id, qty);

        return (await GetByIdAsync(booking.Id))!;
    }

    public async Task<List<BookingDto>> GetUserBookingsAsync(int userId) =>
        await _context.Bookings.Include(b => b.User).Include(b => b.Room).Include(b => b.Hotel)
            .Where(b => b.UserId == userId).OrderByDescending(b => b.CreatedAt)
            .Select(b => Map(b)).ToListAsync();

    public async Task<List<BookingDto>> GetAllBookingsAsync() =>
        await _context.Bookings.Include(b => b.User).Include(b => b.Room).Include(b => b.Hotel)
            .OrderByDescending(b => b.CreatedAt).Select(b => Map(b)).ToListAsync();

    public async Task<BookingDto?> GetByIdAsync(int id)
    {
        var b = await _context.Bookings
            .Include(b => b.User).Include(b => b.Room).Include(b => b.Hotel)
            .FirstOrDefaultAsync(b => b.Id == id);
        return b == null ? null : Map(b);
    }

    public async Task<bool> CancelAsync(int id, int userId, string role)
    {
        var b = await _context.Bookings
            .Include(b => b.User).Include(b => b.Hotel)
            .FirstOrDefaultAsync(b => b.Id == id);
        if (b == null) return false;
        if (role != "Admin" && b.UserId != userId)
            throw new UnauthorizedAccessException("Cannot cancel another user's booking.");
        b.Status = "Cancelled"; b.CancelledAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Booking #{Id} cancelled. Inventory restored.", id);
        await _email.SendCancellationEmailAsync(b.User.Email, b.User.FullName, b.Hotel.Name, b.Id);
        return true;
    }

    private static BookingDto Map(Booking b) => new()
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
        Quantity = b.Quantity,
        Status = b.Status,
        SpecialRequests = b.SpecialRequests,
        CreatedAt = b.CreatedAt,
        CancelledAt = b.CancelledAt
    };
}