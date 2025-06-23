namespace TaskHandler.Infrastructure.Configurations;

public class JwtSettings
{
    public string? Key { get; set; }
    public string? Issuer { get; set; }
    public int AccessTokenLifetimeMinutes { get; set; }
    public int RefreshTokenLifetimeDays { get; set; }
    public string? Audience { get; set; }
}