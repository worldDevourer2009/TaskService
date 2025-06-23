using System.Net;
using System.Net.Mail;
using TaskHandler.Domain.Services;

namespace TaskHandler.Infrastructure.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly string? _smtpServer;
    private readonly int _smtpPort;
    private readonly string? _smtpUser;
    private readonly string? _smtpPassword;
    private readonly string? _smtpFrom;
    private readonly string? _smtpFromDisplayName;
    private readonly bool _smtpEnableSsl;
    
    public SmtpEmailSender(
        string? smtpServer, 
        int smtpPort, 
        string? smtpUser, 
        string? smtpPassword, 
        bool smtpEnableSsl,
        string? smtpFrom, 
        string? smtpFromDisplayName)
    {
        _smtpServer = smtpServer;
        _smtpPort = smtpPort;
        _smtpUser = smtpUser;
        _smtpPassword = smtpPassword;
        _smtpFrom = smtpFrom;
        _smtpFromDisplayName = smtpFromDisplayName;
        _smtpEnableSsl = smtpEnableSsl;
    }
    
    public async Task<bool> SendEmailAsync(string email, string subject, string message)
    {
        return await SendEmailAsync(email, subject, message, null);
    }
    
    public async Task<bool> SendEmailAsync(string email, string subject, string message, string? htmlMessage)
    {
        return await SendEmailAsync(email, subject, message, htmlMessage, null);
    }
    
    public async Task<bool> SendEmailAsync(string email, string subject, string message, string? htmlMessage,
        string[]? attachments)
    {
        try
        {
            if (string.IsNullOrEmpty(_smtpFrom) || string.IsNullOrEmpty(_smtpServer))
            {
                return false;
            }
            
            var mail = new MailMessage
            {
                From = new MailAddress(_smtpFrom, _smtpFromDisplayName ?? _smtpFrom),
                Subject = subject,
                Body = message,
                IsBodyHtml = !string.IsNullOrWhiteSpace(htmlMessage)
            };
            
            mail.To.Add(new MailAddress(email));
            
            if (!string.IsNullOrWhiteSpace(htmlMessage))
            {
                mail.Body = htmlMessage;
                
                if (!string.IsNullOrWhiteSpace(message))
                {
                    var plainView = AlternateView.CreateAlternateViewFromString(message, null, "text/plain");
                    var htmlView = AlternateView.CreateAlternateViewFromString(htmlMessage, null, "text/html");
                    mail.AlternateViews.Add(plainView);
                    mail.AlternateViews.Add(htmlView);
                }
            }
            else
            {
                mail.Body = message;
            }
            
            if (attachments != null && attachments.Length > 0)
            {
                foreach (var attachment in attachments)
                {
                    if (!File.Exists(attachment))
                    {
                        continue;
                    }
                    mail.Attachments.Add(new Attachment(attachment));
                }
            }
            
            using var client = new SmtpClient
            {
                Host = _smtpServer ?? "sandbox.smtp.mailtrap.io",
                Port = _smtpPort,
                EnableSsl = _smtpEnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_smtpUser, _smtpPassword)
            };
            
            await client.SendMailAsync(mail);
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Email sending error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            
            return false;
        }
    }
}