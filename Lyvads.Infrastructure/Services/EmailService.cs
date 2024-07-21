using Lyvads.Domain.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Lyvads.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration configuration)
    {
        _config = configuration;
    }

    public async Task<bool> SendEmailAsync(string recipientEmail, string subject, string body)
    {
        var senderEmail = _config.GetSection("EmailSettings:SenderEmail").Value;
        var port = Convert.ToInt32(_config.GetSection("EmailSettings:Port").Value);
        var host = _config.GetSection("EmailSettings:Host").Value;
        var appPassword = _config.GetSection("EmailSettings:AppPassword").Value;

        var email = new MimeMessage();
        email.Sender = MailboxAddress.Parse(senderEmail);
        email.To.Add(MailboxAddress.Parse(recipientEmail));
        email.Subject = subject;
        var builder = new BodyBuilder();
        builder.HtmlBody = body;
        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        smtp.CheckCertificateRevocation = true;
        await smtp.ConnectAsync(host, port, true);
        await smtp.AuthenticateAsync(senderEmail, appPassword);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);

        return true;
    }
}