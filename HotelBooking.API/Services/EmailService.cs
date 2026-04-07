using MailKit.Net.Smtp;
using MimeKit;

namespace HotelBooking.API.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    private async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        try
        {
            var settings = _config.GetSection("EmailSettings");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(settings["SenderName"], settings["SenderEmail"]));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();
            await client.ConnectAsync(settings["SmtpHost"], int.Parse(settings["SmtpPort"]!), false);
            await client.AuthenticateAsync(settings["SenderEmail"], settings["SenderPassword"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {Email}: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
        }
    }

    public async Task SendRegistrationEmailAsync(string email, string name)
    {
        var body = $@"
        <div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;background:#f8f9fa;padding:30px;border-radius:10px'>
            <div style='background:linear-gradient(135deg,#667eea,#764ba2);padding:20px;border-radius:8px;text-align:center;margin-bottom:20px'>
                <h1 style='color:white;margin:0'>Welcome to Hotel Booking!</h1>
            </div>
            <h2 style='color:#333'>Hello {name}!</h2>
            <p style='color:#555;line-height:1.6'>Your account has been successfully created. You can now browse hotels and make bookings.</p>
            <p style='color:#555'>Thank you for choosing us!</p>
        </div>";

        await SendEmailAsync(email, name, "Welcome to Hotel Booking!", body);
    }

    public async Task SendBookingConfirmationEmailAsync(string email, string name, string hotelName,
        DateTime checkIn, DateTime checkOut, decimal amount, int bookingId)
    {
        var body = $@"
        <div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;background:#f8f9fa;padding:30px;border-radius:10px'>
            <div style='background:linear-gradient(135deg,#11998e,#38ef7d);padding:20px;border-radius:8px;text-align:center;margin-bottom:20px'>
                <h1 style='color:white;margin:0'>Booking Confirmed!</h1>
            </div>
            <h2 style='color:#333'>Hello {name}!</h2>
            <div style='background:white;padding:20px;border-radius:8px;border-left:4px solid #11998e'>
                <p><strong>Booking ID:</strong> #{bookingId}</p>
                <p><strong>Hotel:</strong> {hotelName}</p>
                <p><strong>Check-in:</strong> {checkIn:dd MMM yyyy}</p>
                <p><strong>Check-out:</strong> {checkOut:dd MMM yyyy}</p>
                <p><strong>Total Amount:</strong> ${amount:F2}</p>
            </div>
        </div>";

        await SendEmailAsync(email, name, $"Booking Confirmation #{bookingId}", body);
    }

    public async Task SendCancellationEmailAsync(string email, string name, string hotelName, int bookingId)
    {
        var body = $@"
        <div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;background:#f8f9fa;padding:30px;border-radius:10px'>
            <div style='background:linear-gradient(135deg,#ff6b6b,#ee5a24);padding:20px;border-radius:8px;text-align:center;margin-bottom:20px'>
                <h1 style='color:white;margin:0'>Booking Cancelled</h1>
            </div>
            <h2 style='color:#333'>Hello {name}!</h2>
            <p style='color:#555'>Your booking <strong>#{bookingId}</strong> at <strong>{hotelName}</strong> has been cancelled.</p>
            <p style='color:#555'>We hope to serve you again!</p>
        </div>";

        await SendEmailAsync(email, name, $"Booking Cancellation #{bookingId}", body);
    }
}