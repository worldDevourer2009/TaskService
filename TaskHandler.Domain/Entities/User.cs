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

    private User()
    {
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
    
    public void UpdateEmail(Email email)
    {
        Email = email;
    }

    public void UpdatePassword(string newPassword)
    {
        Password = Password.Create(newPassword);
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
    
    public void Inactivate()
    {
        LastLogin = DateTime.UtcNow;
        IsActive = false;
    }
}
