using System.ComponentModel.DataAnnotations;

namespace HotelBooking.API.DTOs;

public class HotelDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int StarRating { get; set; }
    public string? Amenities { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public int RoomCount { get; set; }
    public decimal MinPrice { get; set; }
}

public class CreateHotelDto
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Range(1, 5)] public int StarRating { get; set; } = 3;
    public string? Amenities { get; set; }
    public string? ImageUrl { get; set; }
}

public class UpdateHotelDto
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Range(1, 5)] public int StarRating { get; set; } = 3;
    public string? Amenities { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
}