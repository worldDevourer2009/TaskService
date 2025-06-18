namespace TaskHandler.Domain.Entities;

public class LoginAttempt
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string? Email { get; private set; }
    public string? Ip { get; private set; }
    public DateTime? Date { get; private set; }
    public bool? Success { get; private set; }
    public string? UserAgent { get; private set; }

    public static LoginAttempt Create(Guid userId,
        string email, string ip, string userAgent)
    {
        return new LoginAttempt()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Email = email,
            Ip = ip,
            UserAgent = userAgent,
            Date = DateTime.Now,
            Success = false
        };
    }
}