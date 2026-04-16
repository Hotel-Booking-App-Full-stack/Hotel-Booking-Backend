using HotelBooking.API.Data;
using HotelBooking.API.DTOs;
using HotelBooking.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.API.Services;

public class RoomService : IRoomService
{
    private readonly AppDbContext _context;
    public RoomService(AppDbContext context) => _context = context;

    private int GetBookedCount(Room r) =>
        r.Bookings.Count(b => b.Status == "Confirmed");

    private RoomDto ToDto(Room r)
    {
        var booked = GetBookedCount(r);
        return new RoomDto
        {
            Id = r.Id,
            HotelId = r.HotelId,
            HotelName = r.Hotel?.Name ?? "",
            RoomType = r.RoomType,
            RoomNumber = r.RoomNumber,
            PricePerNight = r.PricePerNight,
            MaxOccupancy = r.MaxOccupancy,
            Description = r.Description,
            IsAvailable = r.IsAvailable,
            TotalRooms = r.TotalRooms,
            BookedCount = booked,
            AvailableCount = Math.Max(0, r.TotalRooms - booked)
        };
    }

    private IQueryable<Room> BaseQuery() =>
        _context.Rooms.Include(r => r.Hotel).Include(r => r.Bookings);

    public async Task<List<RoomDto>> GetByHotelAsync(int hotelId, string? roomType,
        decimal? minPrice, decimal? maxPrice, bool? availableOnly)
    {
        var q = BaseQuery().Where(r => r.HotelId == hotelId);
        if (!string.IsNullOrEmpty(roomType))
            q = q.Where(r => r.RoomType.ToLower() == roomType.ToLower());
        if (minPrice.HasValue) q = q.Where(r => r.PricePerNight >= minPrice.Value);
        if (maxPrice.HasValue) q = q.Where(r => r.PricePerNight <= maxPrice.Value);

        var list = await q.ToListAsync();
        var result = list.Select(ToDto).ToList();
        if (availableOnly == true) result = result.Where(r => r.AvailableCount > 0).ToList();
        return result;
    }

    public async Task<List<RoomTypeSummaryDto>> GetSummaryByHotelAsync(int hotelId)
    {
        var rooms = await BaseQuery().Where(r => r.HotelId == hotelId).ToListAsync();
        return rooms.GroupBy(r => r.RoomType).Select(g =>
        {
            var dtos = g.Select(ToDto).ToList();
            return new RoomTypeSummaryDto
            {
                RoomType = g.Key,
                TotalRooms = dtos.Sum(d => d.TotalRooms),
                BookedCount = dtos.Sum(d => d.BookedCount),
                AvailableCount = dtos.Sum(d => d.AvailableCount),
                MinPrice = dtos.Min(d => d.PricePerNight),
                Rooms = dtos
            };
        }).ToList();
    }

    public async Task<AdminRoomStatsDto> GetAdminStatsAsync()
    {
        var rooms = await BaseQuery().ToListAsync();
        var dtos = rooms.Select(ToDto).ToList();
        return new AdminRoomStatsDto
        {
            TotalRoomEntries = dtos.Count,
            TotalRoomsCapacity = dtos.Sum(d => d.TotalRooms),
            TotalBooked = dtos.Sum(d => d.BookedCount),
            TotalAvailable = dtos.Sum(d => d.AvailableCount),
            ByType = dtos.GroupBy(d => d.RoomType).Select(g => new RoomTypeStatDto
            {
                RoomType = g.Key,
                TotalRooms = g.Sum(d => d.TotalRooms),
                Booked = g.Sum(d => d.BookedCount),
                Available = g.Sum(d => d.AvailableCount)
            }).ToList()
        };
    }

    public async Task<List<RoomDto>> GetAllAsync() =>
        (await BaseQuery().ToListAsync()).Select(ToDto).ToList();

    public async Task<RoomDto?> GetByIdAsync(int id)
    {
        var r = await BaseQuery().FirstOrDefaultAsync(r => r.Id == id);
        return r == null ? null : ToDto(r);
    }

    public async Task<RoomDto> CreateAsync(CreateRoomDto dto)
    {
        var r = new Room
        {
            HotelId = dto.HotelId,
            RoomType = dto.RoomType,
            RoomNumber = dto.RoomNumber,
            PricePerNight = dto.PricePerNight,
            MaxOccupancy = dto.MaxOccupancy,
            Description = dto.Description,
            TotalRooms = dto.TotalRooms
        };
        _context.Rooms.Add(r);
        await _context.SaveChangesAsync();
        return (await GetByIdAsync(r.Id))!;
    }

    public async Task<RoomDto?> UpdateAsync(int id, UpdateRoomDto dto)
    {
        var r = await _context.Rooms.FindAsync(id);
        if (r == null) return null;
        r.RoomType = dto.RoomType; r.RoomNumber = dto.RoomNumber;
        r.PricePerNight = dto.PricePerNight; r.MaxOccupancy = dto.MaxOccupancy;
        r.Description = dto.Description; r.IsAvailable = dto.IsAvailable;
        r.TotalRooms = dto.TotalRooms;
        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var r = await _context.Rooms.FindAsync(id);
        if (r == null) return false;
        _context.Rooms.Remove(r);
        await _context.SaveChangesAsync();
        return true;
    }
}