using HotelBooking.API.DTOs;
using HotelBooking.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers;

[ApiController, Route("api/[controller]")]
public class RoomController : ControllerBase
{
    private readonly IRoomService _rooms;
    public RoomController(IRoomService rooms) => _rooms = rooms;

    [HttpGet("hotel/{hotelId}")]
    public async Task<IActionResult> GetByHotel(int hotelId,
        [FromQuery] string? roomType, [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice, [FromQuery] bool? availableOnly) =>
        Ok(await _rooms.GetByHotelAsync(hotelId, roomType, minPrice, maxPrice, availableOnly));

    [HttpGet("hotel/{hotelId}/summary")]
    public async Task<IActionResult> Summary(int hotelId) =>
        Ok(await _rooms.GetSummaryByHotelAsync(hotelId));

    [HttpGet("admin/stats"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminStats() => Ok(await _rooms.GetAdminStatsAsync());

    [HttpGet, Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll() => Ok(await _rooms.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var r = await _rooms.GetByIdAsync(id);
        return r == null ? NotFound() : Ok(r);
    }

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateRoomDto dto) =>
        Ok(await _rooms.CreateAsync(dto));

    [HttpPut("{id}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoomDto dto)
    {
        var r = await _rooms.UpdateAsync(id, dto);
        return r == null ? NotFound() : Ok(r);
    }

    [HttpDelete("{id}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id) =>
        await _rooms.DeleteAsync(id) ? Ok(new { message = "Deleted." }) : NotFound();
}