using HotelBooking.API.DTOs;

namespace HotelBooking.API.Services;

public interface IHotelService
{
    Task<List<HotelDto>> GetAllAsync(string? location, int? stars);
    Task<HotelDto?> GetByIdAsync(int id);
    Task<HotelDto> CreateAsync(CreateHotelDto dto);
    Task<HotelDto?> UpdateAsync(int id, UpdateHotelDto dto);
    Task<bool> DeleteAsync(int id);
}