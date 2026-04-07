using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelBooking.API.Models;

[Table("AuditLogs")]
public class AuditLog
{
    [Key]
    public int Id { get; set; }

    public int? UserId { get; set; }

    [Required, MaxLength(200)]
    public string Action { get; set; } = string.Empty;

    public string? Details { get; set; }

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    public int? StatusCode { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}