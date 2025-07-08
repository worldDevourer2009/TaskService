namespace TaskHandler.Application.Configurations;

public class InternalAuthSettings
{
    public string? ServiceClientId { get; set; }
    public string? ServiceClientSecret { get; set; }
    public string? Endpoint { get; set; }
    public string? Scope { get; set; }
    public int? AccessTokenExpirationMinutes { get; set; }
}