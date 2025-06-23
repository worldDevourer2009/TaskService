namespace TaskHandler.Infrastructure.Configurations;

public class EmailSettings
{
    public string? SmtpServer { get; set; }
    public int SmtpPort { get; set; }
    public string? UsernameSmtp { get; set; }
    public string? PasswordSmtp { get; set; }
    public bool EnableSmtpSsl { get; set; }
    public string? FromSmtpName { get; set; }
    public string? FromSmtpDisplayName { get; set; }
}