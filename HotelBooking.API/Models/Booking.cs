using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelBooking.API.Models;

[Table("Bookings")]
public class Booking
{
    [Key] public int Id { get; set; }
    public int UserId { get; set; }
    public int RoomId { get; set; }
    public int HotelId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    [Column(TypeName = "decimal(10,2)")] public decimal TotalAmount { get; set; }
    public int Quantity { get; set; } = 1;
    [MaxLength(50)] public string Status { get; set; } = "Confirmed";
    public string? SpecialRequests { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CancelledAt { get; set; }
    public User User { get; set; } = null!;
    public Room Room { get; set; } = null!;
    public Hotel Hotel { get; set; } = null!;
}