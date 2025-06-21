namespace TaskHandler.Domain.ValueObjects;

public class AccessToken : ValueObject
{
    public string? TokenHash { get; private init; }
    public DateTime ExpirationDate { get; private init; }

    private AccessToken()
    {
    }

    public static AccessToken Create(string tokenHash, TimeSpan expirationDate)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new ArgumentException("Token hash is required", nameof(tokenHash));
        }
        
        if (expirationDate <= TimeSpan.Zero)
        {
            throw new ArgumentException("Expiration must be positive", nameof(expirationDate));
        }
        
        return new AccessToken()
        {
            TokenHash = tokenHash,
            ExpirationDate = DateTime.UtcNow.Add(expirationDate)
        };
    }
        
    public override IEnumerable<object> GetEqualityComponents()
    {
        if (TokenHash != null)
        {
            yield return TokenHash;
        }
        
        yield return ExpirationDate;
    }
}