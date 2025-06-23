using Microsoft.Extensions.Logging;
using Moq;
using TaskHandler.Infrastructure.Configurations;
using TaskHandler.Infrastructure.Services;
using Xunit;

namespace TaskHandler.Tests.Emails;

public class EmailSendingTest
{
    private const bool EnableSending = false;
    
    private readonly SmtpEmailSender _emailSender;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public EmailSendingTest()
    {
        _logger = Mock.Of<ILogger<SmtpEmailSender>>();
        Mock.Get(_logger).Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        
        _emailSettings = new EmailSettings()
        {
            EnableSmtpSsl = true,
            FromSmtpDisplayName = "TaskHandler App",
            FromSmtpName = "from@example.com",
            PasswordSmtp = "10e7040d944199",
            SmtpPort = 2525,
            SmtpServer = "sandbox.smtp.mailtrap.io",
            UsernameSmtp = "5d0308f8286c0e"
        };

        _emailSender = new SmtpEmailSender(
            _emailSettings.SmtpServer, 
            _emailSettings.SmtpPort,
            _emailSettings.UsernameSmtp,
            _emailSettings.PasswordSmtp,
            _emailSettings.EnableSmtpSsl,
            _emailSettings.FromSmtpName, 
            _emailSettings.FromSmtpDisplayName);
    }

    [Fact]
    public async Task Send_TestEmail()
    {
        if (!EnableSending)
        {
            return;
        }
        
        var result = await _emailSender.SendEmailAsync("test@gmail.com", "Test", "Test");
        Assert.True(result);
    }

    [Fact]
    public void EmailSender_Constructor_ValidSettings_ReturnsValidEmailSender()
    {
        var emailSender = new SmtpEmailSender(
            _emailSettings.SmtpServer, 
            _emailSettings.SmtpPort,
            _emailSettings.UsernameSmtp,
            _emailSettings.PasswordSmtp,
            _emailSettings.EnableSmtpSsl,
            _emailSettings.FromSmtpName, 
            _emailSettings.FromSmtpDisplayName);
        
        Assert.NotNull(emailSender);
    }
}