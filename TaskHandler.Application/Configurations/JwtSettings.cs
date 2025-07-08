namespace TaskHandler.Application.Configurations;

public class JwtSettings
{
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public string? FallbackKey { get; set; }
}