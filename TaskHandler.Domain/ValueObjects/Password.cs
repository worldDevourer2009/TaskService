namespace TaskHandler.Domain.ValueObjects;

public class Password : ValueObject
{
    public string Hash { get; }
    
    private Password(string hash)
    {
        Hash = hash;
    }

    public static Password Create(string password)
    {
        if (password.Length is < 8 or > 100)
        {
            throw new ArgumentException("Password must be at least 8 characters long", nameof(password));
        }

        if (password.Contains(' '))
        {
            throw new ArgumentException("Password cannot contain spaces", nameof(password));
        }

        if (password.Contains('\n'))
        {
            throw new ArgumentException("Password cannot contain new lines", nameof(password));
        }
        
        if (!password.Any(char.IsDigit))
        {
            throw new ArgumentException("Password must contain at least one digit", nameof(password));
        }

        if (!password.All(char.IsAsciiLetterOrDigit))
        {
            throw new ArgumentException("Password must contain only ASCII letters and digits", nameof(password));
        }
        
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        return new Password(hash);
    }

    public static Password FromHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            throw new ArgumentException("Hash cannot be null or whitespace", nameof(hash));
        }
        return new Password(hash);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Hash;
    }
}