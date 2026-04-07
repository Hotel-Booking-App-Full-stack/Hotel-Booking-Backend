using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelBooking.API.Models;

[Table("Rooms")]
public class Room
{
    [Key]
    public int Id { get; set; }

    public int HotelId { get; set; }

    [Required, MaxLength(100)]
    public string RoomType { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string RoomNumber { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    public decimal PricePerNight { get; set; }

    public int MaxOccupancy { get; set; } = 2;

    public string? Description { get; set; }

    public bool IsAvailable { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Hotel Hotel { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}