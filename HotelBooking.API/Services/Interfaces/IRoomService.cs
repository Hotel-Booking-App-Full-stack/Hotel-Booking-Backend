using HotelBooking.API.DTOs;

namespace HotelBooking.API.Services;

public interface IRoomService
{
    Task<List<RoomDto>> GetByHotelAsync(int hotelId, string? roomType,
        decimal? minPrice, decimal? maxPrice, bool? availableOnly);
    Task<List<RoomTypeSummaryDto>> GetSummaryByHotelAsync(int hotelId);
    Task<AdminRoomStatsDto> GetAdminStatsAsync();
    Task<List<RoomDto>> GetAllAsync();
    Task<RoomDto?> GetByIdAsync(int id);
    Task<RoomDto> CreateAsync(CreateRoomDto dto);
    Task<RoomDto?> UpdateAsync(int id, UpdateRoomDto dto);
    Task<bool> DeleteAsync(int id);
}