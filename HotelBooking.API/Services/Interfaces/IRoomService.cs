using HotelBooking.API.DTOs;

namespace HotelBooking.API.Services;

public interface IRoomService
{
    Task<List<RoomDto>> GetByHotelAsync(int hotelId);
    Task<RoomDto?> GetByIdAsync(int id);
    Task<RoomDto> CreateAsync(CreateRoomDto dto);
    Task<RoomDto?> UpdateAsync(int id, UpdateRoomDto dto);
    Task<bool> DeleteAsync(int id);
}