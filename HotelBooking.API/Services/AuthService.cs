using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HotelBooking.API.Data;
using HotelBooking.API.DTOs;
using HotelBooking.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HotelBooking.API.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(AppDbContext context, IConfiguration config,
        IEmailService emailService, ILogger<AuthService> logger)
    {
        _context = context; _config = config;
        _emailService = emailService; _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            throw new InvalidOperationException("Email already registered.");

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Password = dto.Password,
            Role = "User",
            IsEmailVerified = false,
            EmailVerificationToken = token,
            EmailVerificationExpiry = DateTime.UtcNow.AddHours(24)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Registered: {Email}", dto.Email);

        // Send BOTH emails
        await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName);
        await _emailService.SendVerificationEmailAsync(user.Email, user.FullName, token);

        await LogAsync(user.Id, "Register", $"{user.Email} registered");
        return GenerateToken(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Password == dto.Password && u.IsActive)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        _logger.LogInformation("Login: {Email}", dto.Email);
        await LogAsync(user.Id, "Login", $"{user.Email} logged in");
        return GenerateToken(user);
    }

    public async Task<bool> VerifyEmailAsync(VerifyEmailDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.Email == dto.Email &&
            u.EmailVerificationToken == dto.Token &&
            u.EmailVerificationExpiry > DateTime.UtcNow);

        if (user == null) return false;
        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationExpiry = null;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Email verified: {Email}", dto.Email);
        return true;
    }

    public async Task<bool> ResendVerificationAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || user.IsEmailVerified) return false;
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        user.EmailVerificationToken = token;
        user.EmailVerificationExpiry = DateTime.UtcNow.AddHours(24);
        await _context.SaveChangesAsync();
        await _emailService.SendVerificationEmailAsync(user.Email, user.FullName, token);
        return true;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        return await _context.Users
            .Include(u => u.Bookings)
            .Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive,
                IsEmailVerified = u.IsEmailVerified,
                CreatedAt = u.CreatedAt,
                BookingCount = u.Bookings.Count
            }).ToListAsync();
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleUserStatusAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;
        user.IsActive = !user.IsActive;
        await _context.SaveChangesAsync();
        return true;
    }

    private AuthResponseDto GenerateToken(User user)
    {
        var jwt = _config.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!));
        var expiry = DateTime.UtcNow.AddMinutes(double.Parse(jwt["ExpiryMinutes"]!));
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role)
        };
        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"], audience: jwt["Audience"],
            claims: claims, expires: expiry,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            UserId = user.Id,
            ExpiresAt = expiry,
            IsEmailVerified = user.IsEmailVerified
        };
    }

    private async Task LogAsync(int? userId, string action, string details)
    {
        _context.AuditLogs.Add(new AuditLog { UserId = userId, Action = action, Details = details });
        await _context.SaveChangesAsync();
    }
}