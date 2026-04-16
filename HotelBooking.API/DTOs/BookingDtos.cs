using System.ComponentModel.DataAnnotations;

namespace HotelBooking.API.DTOs;

public class CreateBookingDto
{
    [Required] public int RoomId { get; set; }
    [Required] public int HotelId { get; set; }
    [Required] public DateTime CheckInDate { get; set; }
    [Required] public DateTime CheckOutDate { get; set; }
    [Range(1, 50)] public int Quantity { get; set; } = 1;
    public string? SpecialRequests { get; set; }
}

public class BookingDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public int HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public string HotelLocation { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public decimal TotalAmount { get; set; }
    public int Quantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? SpecialRequests { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}