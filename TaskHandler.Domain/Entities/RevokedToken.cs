using TaskHandler.Domain.ValueObjects;

namespace TaskHandler.Domain.Entities;

public class RevokedToken : ValueObject
{
    public string TokenId { get; private set; } = null!;
    public string Token { get; private set; } = null!;
    public Guid? UserId { get; private set; }
    public DateTime RevokedAt { get; private set; }
    public DateTime? ExpirationDate { get; private set; }

    private RevokedToken()
    {
    }

    public static RevokedToken Create(string tokenId, string token, Guid? userId, DateTime revokedAt,
        DateTime? expirationDate)
    {
        return new RevokedToken()
        {
            TokenId = tokenId,
            Token = token,
            UserId = userId,
            RevokedAt = revokedAt,
            ExpirationDate = expirationDate
        };
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        throw new NotImplementedException();
    }
}