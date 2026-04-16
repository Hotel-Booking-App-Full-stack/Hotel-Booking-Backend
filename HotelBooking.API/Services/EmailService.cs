using MailKit.Net.Smtp;
using MimeKit;

namespace HotelBooking.API.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    { _config = config; _logger = logger; }

    private async Task SendAsync(string to, string name, string subject, string html)
    {
        try
        {
            var s = _config.GetSection("EmailSettings");
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(s["SenderName"], s["SenderEmail"]));
            msg.To.Add(new MailboxAddress(name, to));
            msg.Subject = subject;
            msg.Body = new TextPart("html") { Text = html };
            using var client = new SmtpClient();
            await client.ConnectAsync(s["SmtpHost"], int.Parse(s["SmtpPort"]!), false);
            await client.AuthenticateAsync(s["SenderEmail"], s["SenderPassword"]);
            await client.SendAsync(msg);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex) { _logger.LogError(ex, "Email failed to {To}", to); }
    }

    private static string BaseLayout(string content) => $@"
    <div style='font-family:Inter,Segoe UI,sans-serif;max-width:580px;margin:0 auto;background:#f8fafc;border-radius:12px;overflow:hidden;border:1px solid #e2e8f0'>
      <div style='background:#0f172a;padding:24px 32px;display:flex;align-items:center;gap:12px'>
        <div style='width:36px;height:36px;background:#059669;border-radius:8px;display:flex;align-items:center;justify-content:center;font-size:18px;color:#fff;font-weight:800'>H</div>
        <div><div style='color:#f8fafc;font-weight:700;font-size:18px'>HotelPro</div><div style='color:#475569;font-size:11px'>Premium Hotel Bookings</div></div>
      </div>
      <div style='padding:32px;background:#fff'>{content}</div>
      <div style='background:#f1f5f9;padding:16px 32px;text-align:center;border-top:1px solid #e2e8f0'>
        <p style='color:#94a3b8;font-size:11px;margin:0'>© 2024 HotelPro · All rights reserved</p>
      </div>
    </div>";

    public async Task SendWelcomeEmailAsync(string email, string name)
    {
        var body = BaseLayout($@"
        <h2 style='color:#0f172a;font-size:20px;margin-bottom:12px'>Welcome, {name}!</h2>
        <p style='color:#475569;line-height:1.7'>Your HotelPro account has been created. Browse and book from hundreds of premium hotels worldwide.</p>
        <p style='color:#475569;margin-top:16px'>Please check your inbox to verify your email address.</p>");
        await SendAsync(email, name, "Welcome to HotelPro!", body);
    }

    public async Task SendVerificationEmailAsync(string email, string name, string token)
    {
        var baseUrl = _config["AppSettings:FrontendUrl"] ?? "http://localhost:4200";
        var url = $"{baseUrl}/verify-email?token={token}&email={Uri.EscapeDataString(email)}";
        var body = BaseLayout($@"
        <h2 style='color:#0f172a;font-size:20px;margin-bottom:12px'>Verify Your Email</h2>
        <p style='color:#475569;line-height:1.7'>Hello <strong>{name}</strong>, click the button below to verify your email address and activate your account.</p>
        <div style='text-align:center;margin:28px 0'>
          <a href='{url}' style='background:#059669;color:#fff;padding:14px 36px;border-radius:8px;font-weight:700;font-size:15px;text-decoration:none;display:inline-block'>Verify Email Address</a>
        </div>
        <p style='color:#94a3b8;font-size:12px'>This link expires in 24 hours.</p>");
        await SendAsync(email, name, "Verify your HotelPro email", body);
    }

    public async Task SendBookingConfirmationEmailAsync(string email, string name, string hotelName,
        DateTime checkIn, DateTime checkOut, decimal amount, int bookingId, int qty)
    {
        var body = BaseLayout($@"
        <h2 style='color:#059669;font-size:20px;margin-bottom:16px'>Booking Confirmed!</h2>
        <div style='background:#f0fdf4;border:1px solid #bbf7d0;border-radius:8px;padding:20px;border-left:4px solid #059669'>
          <table style='width:100%;font-size:14px;color:#374151'>
            <tr><td style='padding:6px 0;color:#6b7280'>Booking ID</td><td style='text-align:right;font-weight:700'>#{bookingId}</td></tr>
            <tr><td style='padding:6px 0;color:#6b7280'>Hotel</td><td style='text-align:right;font-weight:700'>{hotelName}</td></tr>
            <tr><td style='padding:6px 0;color:#6b7280'>Rooms</td><td style='text-align:right;font-weight:700'>{qty}</td></tr>
            <tr><td style='padding:6px 0;color:#6b7280'>Check-in</td><td style='text-align:right'>{checkIn:dd MMM yyyy}</td></tr>
            <tr><td style='padding:6px 0;color:#6b7280'>Check-out</td><td style='text-align:right'>{checkOut:dd MMM yyyy}</td></tr>
            <tr style='border-top:1px solid #e5e7eb'><td style='padding:12px 0 0;font-weight:700;font-size:16px'>Total</td><td style='text-align:right;font-weight:800;font-size:20px;color:#059669'>${amount:F2}</td></tr>
          </table>
        </div>");
        await SendAsync(email, name, $"Booking #{bookingId} Confirmed — HotelPro", body);
    }

    public async Task SendCancellationEmailAsync(string email, string name, string hotelName, int bookingId)
    {
        var body = BaseLayout($@"
        <h2 style='color:#dc2626;font-size:20px;margin-bottom:12px'>Booking Cancelled</h2>
        <p style='color:#475569;line-height:1.7'>Hello <strong>{name}</strong>, your booking <strong>#{bookingId}</strong> at <strong>{hotelName}</strong> has been cancelled. We hope to see you again soon.</p>");
        await SendAsync(email, name, $"Booking #{bookingId} Cancelled — HotelPro", body);
    }
}