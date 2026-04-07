using HotelBooking.API.DTOs;
using HotelBooking.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService) => _bookingService = bookingService;

    private int GetUserId() =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    private string GetUserRole() =>
        User.FindFirst(ClaimTypes.Role)!.Value;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingDto dto)
    {
        var booking = await _bookingService.CreateAsync(GetUserId(), dto);
        return Ok(booking);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyBookings() =>
        Ok(await _bookingService.GetUserBookingsAsync(GetUserId()));

    [HttpGet, Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll() =>
        Ok(await _bookingService.GetAllBookingsAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var booking = await _bookingService.GetByIdAsync(id);
        return booking == null ? NotFound() : Ok(booking);
    }

    [HttpDelete("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _bookingService.CancelAsync(id, GetUserId(), GetUserRole());
        return result ? Ok(new { message = "Booking cancelled." }) : NotFound();
    }
}