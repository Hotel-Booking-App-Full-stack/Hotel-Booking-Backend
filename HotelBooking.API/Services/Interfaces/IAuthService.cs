using HotelBooking.API.DTOs;

namespace HotelBooking.API.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<bool> VerifyEmailAsync(VerifyEmailDto dto);
    Task<bool> ResendVerificationAsync(string email);
    Task<List<UserDto>> GetAllUsersAsync();
    Task<bool> DeleteUserAsync(int id);
    Task<bool> ToggleUserStatusAsync(int id);
}