using HotelBooking.API.Data;
using HotelBooking.API.DTOs;
using HotelBooking.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.API.Services;

public class HotelService : IHotelService
{
    private readonly AppDbContext _context;
    private readonly ILogger<HotelService> _logger;

    public HotelService(AppDbContext context, ILogger<HotelService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<HotelDto>> GetAllAsync(string? location, int? stars)
    {
        var query = _context.Hotels.Include(h => h.Rooms).Where(h => h.IsActive).AsQueryable();

        if (!string.IsNullOrEmpty(location))
            query = query.Where(h => h.Location.Contains(location));

        if (stars.HasValue)
            query = query.Where(h => h.StarRating == stars.Value);

        return await query.Select(h => new HotelDto
        {
            Id = h.Id,
            Name = h.Name,
            Location = h.Location,
            Description = h.Description,
            StarRating = h.StarRating,
            Amenities = h.Amenities,
            ImageUrl = h.ImageUrl,
            IsActive = h.IsActive,
            RoomCount = h.Rooms.Count(r => r.IsAvailable),
            MinPrice = h.Rooms.Any() ? h.Rooms.Min(r => r.PricePerNight) : 0
        }).ToListAsync();
    }

    public async Task<HotelDto?> GetByIdAsync(int id)
    {
        var h = await _context.Hotels.Include(h => h.Rooms).FirstOrDefaultAsync(h => h.Id == id);
        if (h == null) return null;

        return new HotelDto
        {
            Id = h.Id,
            Name = h.Name,
            Location = h.Location,
            Description = h.Description,
            StarRating = h.StarRating,
            Amenities = h.Amenities,
            ImageUrl = h.ImageUrl,
            IsActive = h.IsActive,
            RoomCount = h.Rooms.Count(r => r.IsAvailable),
            MinPrice = h.Rooms.Any() ? h.Rooms.Min(r => r.PricePerNight) : 0
        };
    }

    public async Task<HotelDto> CreateAsync(CreateHotelDto dto)
    {
        var hotel = new Hotel
        {
            Name = dto.Name,
            Location = dto.Location,
            Description = dto.Description,
            StarRating = dto.StarRating,
            Amenities = dto.Amenities,
            ImageUrl = dto.ImageUrl
        };
        _context.Hotels.Add(hotel);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Hotel created: {Name}", hotel.Name);
        return (await GetByIdAsync(hotel.Id))!;
    }

    public async Task<HotelDto?> UpdateAsync(int id, UpdateHotelDto dto)
    {
        var hotel = await _context.Hotels.FindAsync(id);
        if (hotel == null) return null;

        hotel.Name = dto.Name;
        hotel.Location = dto.Location;
        hotel.Description = dto.Description;
        hotel.StarRating = dto.StarRating;
        hotel.Amenities = dto.Amenities;
        hotel.ImageUrl = dto.ImageUrl;
        hotel.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var hotel = await _context.Hotels.FindAsync(id);
        if (hotel == null) return false;
        hotel.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }
}