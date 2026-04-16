using System.Security.Claims;
using HotelBooking.API.DTOs;
using HotelBooking.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers;

[ApiController, Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto) =>
        Ok(await _auth.RegisterAsync(dto));

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto) =>
        Ok(await _auth.LoginAsync(dto));

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto) =>
        await _auth.VerifyEmailAsync(dto)
            ? Ok(new { message = "Email verified successfully." })
            : BadRequest(new { message = "Invalid or expired token." });

    [HttpPost("resend-verification")]
    public async Task<IActionResult> Resend([FromBody] string email)
    {
        await _auth.ResendVerificationAsync(email);
        return Ok(new { message = "Verification email sent." });
    }

    [HttpGet("users"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsers() => Ok(await _auth.GetAllUsersAsync());

    [HttpDelete("users/{id}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(int id) =>
        await _auth.DeleteUserAsync(id) ? Ok(new { message = "Deleted." }) : NotFound();

    [HttpPatch("users/{id}/toggle"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleUser(int id) =>
        await _auth.ToggleUserStatusAsync(id) ? Ok(new { message = "Toggled." }) : NotFound();

    [HttpGet("me"), Authorize]
    public IActionResult Me() => Ok(new
    {
        userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
        email = User.FindFirst(ClaimTypes.Email)?.Value,
        name = User.FindFirst(ClaimTypes.Name)?.Value,
        role = User.FindFirst(ClaimTypes.Role)?.Value
    });
}