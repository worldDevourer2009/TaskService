namespace TaskHandler.Domain.ValueObjects;

public class Email : ValueObject
{
    public string Value { get; }

    private Email(string emailAddress)
    {
        Value = emailAddress;
    }

    public static Email Create(string emailAddress)
    {
        if (string.IsNullOrWhiteSpace(emailAddress))
        {
            throw new ArgumentException("Email address cannot be null or whitespace", nameof(emailAddress));
        }

        if (!emailAddress.Contains('@') || !emailAddress.Contains('.'))
        {
            throw new ArgumentException("Email address is not valid", nameof(emailAddress));
        }
        
        return new Email(emailAddress);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value.ToLowerInvariant();
    }

    public override string ToString()
    {
        return Value;   
    }
}