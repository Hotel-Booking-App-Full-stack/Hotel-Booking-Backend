using HotelBooking.API.DTOs;
using HotelBooking.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HotelController : ControllerBase
{
    private readonly IHotelService _hotelService;

    public HotelController(IHotelService hotelService) => _hotelService = hotelService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? location, [FromQuery] int? stars) =>
        Ok(await _hotelService.GetAllAsync(location, stars));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var hotel = await _hotelService.GetByIdAsync(id);
        return hotel == null ? NotFound() : Ok(hotel);
    }

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateHotelDto dto)
    {
        var hotel = await _hotelService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = hotel.Id }, hotel);
    }

    [HttpPut("{id}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateHotelDto dto)
    {
        var hotel = await _hotelService.UpdateAsync(id, dto);
        return hotel == null ? NotFound() : Ok(hotel);
    }

    [HttpDelete("{id}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _hotelService.DeleteAsync(id);
        return result ? Ok(new { message = "Hotel deactivated." }) : NotFound();
    }
}