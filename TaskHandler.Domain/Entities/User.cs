using TaskHandler.Domain.ValueObjects;

namespace TaskHandler.Domain.Entities;

public class User : Entity
{
    public Email? Email { get; private set; }
    public Password? Password { get; private set; }
    public DateTime? CreatedAt { get; private set; }
    public DateTime? LastLogin { get; private set; }
    public bool IsActive { get; private set; }
    public string? Name { get; private set; }
    public IReadOnlyCollection<PasswordResetToken> PasswordResetTokens => _passwordResetTokens;
    
    private readonly List<PasswordResetToken> _passwordResetTokens;

    private User()
    {
        _passwordResetTokens = new List<PasswordResetToken>();
    }

    public static User Create(Email email, Password password, string name)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Password = password,
            Name = name,
            CreatedAt = DateTime.UtcNow,
            LastLogin = DateTime.UtcNow,
            IsActive = true
        };
    }

    public void AddResetToken(PasswordResetToken passwordResetToken)
    {
        _passwordResetTokens.Add(passwordResetToken);
    }

    public void RemoveResetToken(PasswordResetToken passwordResetToken)
    {
        _passwordResetTokens.Remove(passwordResetToken);
    }

    public PasswordResetToken? GetValidResetToken()
    {
        return _passwordResetTokens.FirstOrDefault(x => x.TokenHash != null && !x.IsExpired(x.TokenHash));   
    }
    
    public void UpdateEmail(Email email)
    {
        Email = email;
    }

    public void UpdatePassword(string newPassword)
    {
        Password = Password.Create(newPassword);
        _passwordResetTokens.Clear();
    }
    
    public void UpdateName(string name)
    {
        Name = name;
    }
    
    public void UpdateLastLogin()
    {
        LastLogin = DateTime.UtcNow;
        IsActive = true;
    }
    
    public void Logout()
    {
        LastLogin = DateTime.UtcNow;
        IsActive = false;
    }
}
