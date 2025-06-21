using TaskHandler.Application.Interfaces;
using TaskHandler.Domain.Repositories;

namespace TaskHandler.Infrastructure.Repositories;

public class RevokedTokenRepository : IRevokedTokenRepository
{
    private readonly IApplicationDbContext _context;

    public RevokedTokenRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<string> GetRevokedToken(string token)
    {
        throw new NotImplementedException();
    }

    public Task RevokeToken(string token)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsRevoked(string token)
    {
        throw new NotImplementedException();
    }
}

public class TokenRepository : ITokenRepository
{
    private readonly IApplicationDbContext _context;
    
    public Task AddToken(string token)
    {
        throw new NotImplementedException();
    }

    public Task<string?> GetToken(string token)
    {
        throw new NotImplementedException();
    }

    public Task RemoveToken(string token)
    {
        throw new NotImplementedException();
    }
}