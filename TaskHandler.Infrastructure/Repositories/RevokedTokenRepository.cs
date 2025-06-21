using Microsoft.EntityFrameworkCore;
using TaskHandler.Application.Interfaces;
using TaskHandler.Domain.Repositories;
using TaskHandler.Domain.ValueObjects;

namespace TaskHandler.Infrastructure.Repositories;

public class RevokedRefreshTokenRepository : IRevokedRefreshTokenRepository
{
    private readonly IApplicationDbContext _context;

    public RevokedRefreshTokenRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken> GetRevokedToken(string token)
    {
        var gottenToken = await _context.RevokedTokens.FirstOrDefaultAsync(x => x.TokenHash == token);

        if (gottenToken == null)
        {
            throw new Exception("Token not found");
        }

        return gottenToken;
    }

    public async Task RevokeToken(string token)
    {
        var gottenToken = await _context.RevokedTokens.FirstOrDefaultAsync(x => x.TokenHash == token);
        
        if (gottenToken == null)
        {
            throw new Exception("Token not found");
        }
        
        _context.RevokedTokens.Remove(gottenToken);
    }

    public async Task<bool> IsRevoked(string token)
    {
        var gottenToken = await _context.RevokedTokens.FirstOrDefaultAsync(x => x.TokenHash == token);
        return gottenToken != null;
    }
}