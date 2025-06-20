using System.Security.Cryptography;

namespace TaskHandler.Domain.ValueObjects;

public class PasswordResetToken : ValueObject
{
    public string TokenHash { get; private init; }
    public DateTime ExpirationDate { get; private init;}
    
    private PasswordResetToken()
    {
    }

    public static (PasswordResetToken token, string plain) Create(TimeSpan lifeTime)
    {
        var plain = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var hash = BCrypt.Net.BCrypt.HashPassword(plain);
        return (new PasswordResetToken {TokenHash = hash, ExpirationDate = DateTime.UtcNow.Add(lifeTime)}, plain);
    }

    public bool IsExpired(string plainToken)
    {
        return DateTime.UtcNow > ExpirationDate && BCrypt.Net.BCrypt.Verify(plainToken, TokenHash);
    }
    
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return TokenHash;
        yield return ExpirationDate;
    }
}