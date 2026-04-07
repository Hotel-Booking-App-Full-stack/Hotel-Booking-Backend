using HotelBooking.API.DTOs;
using HotelBooking.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomController(IRoomService roomService) => _roomService = roomService;

    [HttpGet("hotel/{hotelId}")]
    public async Task<IActionResult> GetByHotel(int hotelId) =>
        Ok(await _roomService.GetByHotelAsync(hotelId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var room = await _roomService.GetByIdAsync(id);
        return room == null ? NotFound() : Ok(room);
    }

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateRoomDto dto) =>
        Ok(await _roomService.CreateAsync(dto));

    [HttpPut("{id}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoomDto dto)
    {
        var room = await _roomService.UpdateAsync(id, dto);
        return room == null ? NotFound() : Ok(room);
    }

    [HttpDelete("{id}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _roomService.DeleteAsync(id);
        return result ? Ok(new { message = "Room deleted." }) : NotFound();
    }
}