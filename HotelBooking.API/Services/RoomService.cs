using HotelBooking.API.Data;
using HotelBooking.API.DTOs;
using HotelBooking.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.API.Services;

public class RoomService : IRoomService
{
    private readonly AppDbContext _context;

    public RoomService(AppDbContext context) => _context = context;

    public async Task<List<RoomDto>> GetByHotelAsync(int hotelId)
    {
        return await _context.Rooms
            .Include(r => r.Hotel)
            .Where(r => r.HotelId == hotelId)
            .Select(r => MapToDto(r))
            .ToListAsync();
    }

    public async Task<RoomDto?> GetByIdAsync(int id)
    {
        var r = await _context.Rooms.Include(r => r.Hotel).FirstOrDefaultAsync(r => r.Id == id);
        return r == null ? null : MapToDto(r);
    }

    public async Task<RoomDto> CreateAsync(CreateRoomDto dto)
    {
        var room = new Room
        {
            HotelId = dto.HotelId,
            RoomType = dto.RoomType,
            RoomNumber = dto.RoomNumber,
            PricePerNight = dto.PricePerNight,
            MaxOccupancy = dto.MaxOccupancy,
            Description = dto.Description
        };
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        return (await GetByIdAsync(room.Id))!;
    }

    public async Task<RoomDto?> UpdateAsync(int id, UpdateRoomDto dto)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null) return null;

        room.RoomType = dto.RoomType;
        room.RoomNumber = dto.RoomNumber;
        room.PricePerNight = dto.PricePerNight;
        room.MaxOccupancy = dto.MaxOccupancy;
        room.Description = dto.Description;
        room.IsAvailable = dto.IsAvailable;

        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null) return false;
        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
        return true;
    }

    private static RoomDto MapToDto(Room r) => new()
    {
        Id = r.Id,
        HotelId = r.HotelId,
        HotelName = r.Hotel?.Name ?? "",
        RoomType = r.RoomType,
        RoomNumber = r.RoomNumber,
        PricePerNight = r.PricePerNight,
        MaxOccupancy = r.MaxOccupancy,
        Description = r.Description,
        IsAvailable = r.IsAvailable
    };
}