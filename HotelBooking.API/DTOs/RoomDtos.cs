using System.ComponentModel.DataAnnotations;

namespace HotelBooking.API.DTOs;

public class RoomDto
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public int MaxOccupancy { get; set; }
    public string? Description { get; set; }
    public bool IsAvailable { get; set; }
    public int TotalRooms { get; set; }
    public int BookedCount { get; set; }
    public int AvailableCount { get; set; }
}

public class RoomTypeSummaryDto
{
    public string RoomType { get; set; } = string.Empty;
    public int TotalRooms { get; set; }
    public int BookedCount { get; set; }
    public int AvailableCount { get; set; }
    public decimal MinPrice { get; set; }
    public List<RoomDto> Rooms { get; set; } = new();
}

public class AdminRoomStatsDto
{
    public int TotalRoomEntries { get; set; }
    public int TotalRoomsCapacity { get; set; }
    public int TotalBooked { get; set; }
    public int TotalAvailable { get; set; }
    public List<RoomTypeStatDto> ByType { get; set; } = new();
}

public class RoomTypeStatDto
{
    public string RoomType { get; set; } = string.Empty;
    public int TotalRooms { get; set; }
    public int Booked { get; set; }
    public int Available { get; set; }
}

public class CreateRoomDto
{
    [Required] public int HotelId { get; set; }
    [Required] public string RoomType { get; set; } = string.Empty;
    [Required] public string RoomNumber { get; set; } = string.Empty;
    [Range(1, 99999)] public decimal PricePerNight { get; set; }
    [Range(1, 20)] public int MaxOccupancy { get; set; } = 2;
    public string? Description { get; set; }
    [Range(1, 200)] public int TotalRooms { get; set; } = 10;
}

public class UpdateRoomDto
{
    [Required] public string RoomType { get; set; } = string.Empty;
    [Required] public string RoomNumber { get; set; } = string.Empty;
    [Range(1, 99999)] public decimal PricePerNight { get; set; }
    [Range(1, 20)] public int MaxOccupancy { get; set; } = 2;
    public string? Description { get; set; }
    public bool IsAvailable { get; set; } = true;
    [Range(1, 200)] public int TotalRooms { get; set; } = 10;
}