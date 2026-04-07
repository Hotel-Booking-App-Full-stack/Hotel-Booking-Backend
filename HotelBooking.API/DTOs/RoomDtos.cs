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
}

public class CreateRoomDto
{
    [Required] public int HotelId { get; set; }
    [Required] public string RoomType { get; set; } = string.Empty;
    [Required] public string RoomNumber { get; set; } = string.Empty;
    [Range(1, 10000)] public decimal PricePerNight { get; set; }
    [Range(1, 20)] public int MaxOccupancy { get; set; } = 2;
    public string? Description { get; set; }
}

public class UpdateRoomDto
{
    [Required] public string RoomType { get; set; } = string.Empty;
    [Required] public string RoomNumber { get; set; } = string.Empty;
    [Range(1, 10000)] public decimal PricePerNight { get; set; }
    [Range(1, 20)] public int MaxOccupancy { get; set; } = 2;
    public string? Description { get; set; }
    public bool IsAvailable { get; set; } = true;
}