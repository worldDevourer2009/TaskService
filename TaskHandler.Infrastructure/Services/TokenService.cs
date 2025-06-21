using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using TaskHandler.Application.Interfaces;
using TaskHandler.Domain.Repositories;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace TaskHandler.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
    private readonly ILogger<TokenService> _logger;
    private readonly string? _secretKey;
    private readonly string? _issuer;

    public TokenService(IUserRepository userRepository, IConfiguration configuration, ILogger<TokenService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;

        _secretKey = configuration["JwtSettings:Key"];

        if (string.IsNullOrEmpty(_secretKey))
        {
            throw new Exception("JwtSettings:Key is required");
        }

        _issuer = configuration["JwtSettings:Issuer"];

        if (string.IsNullOrEmpty(_issuer))
        {
            throw new Exception("JwtSettings:Issuer is required");
        }
        
        _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
    }

    public async Task<string> GenerateToken(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetById(userId, cancellationToken);

        if (user == null)
        {
            throw new Exception("User not found, can't create token");
        }

        if (user.LastLogin == null || user.Email == null || user.Name == null || user.LastLogin == null)
        {
            throw new Exception("User has invalid data");
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim(JwtRegisteredClaimNames.AuthTime, user.LastLogin.ToString() ?? string.Empty)
        };

        if (_secretKey == null || _issuer == null)
        {
            throw new Exception("JwtSettings:Key or JwtSettings:Issuer is required");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(_issuer, user.Id.ToString(), claims, expires: DateTime.Now.AddMinutes(60),
            signingCredentials: credentials);
        
        return _jwtSecurityTokenHandler.WriteToken(token);
    }

    public Task<Guid> GetUserIdFromToken(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentNullException("Token is required" + nameof(token));
        }

        if (!_jwtSecurityTokenHandler.CanReadToken(token))
        {
            throw new Exception("Token is invalid");
        }
        
        var jwtToken = _jwtSecurityTokenHandler.ReadJwtToken(token);
        var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);

        if (subClaim == null)
        {
            throw new Exception("Can't read user id from token");
        }

        if (!Guid.TryParse(subClaim.Value, out var userId))
        {
            throw new Exception("Can't parse user id from token");
        }
        
        return Task.FromResult(userId);
    }

    public Task<bool> ValidateToken(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentNullException("Token is required" + nameof(token));
        }
        
        if (_secretKey == null || _issuer == null)
        {
            throw new Exception("JwtSettings:Key or JwtSettings:Issuer is required");
        }

        if (!_jwtSecurityTokenHandler.CanReadToken(token))
        {
            return Task.FromResult(false);
        }

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var validatedToken =
                _jwtSecurityTokenHandler.ValidateToken(token, validationParameters, out var securityToken);
            return Task.FromResult(true);
        }
        catch (SecurityTokenException)
        {
            _logger.LogError("Token validation error");
            return Task.FromResult(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Token validation error");
            return Task.FromResult(false);
        }
    }

    public Task<bool> RevokeToken(string token, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RevokeTokenForUser(Guid userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RevokeAllTokensForUser(Guid userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RevokeAllTokens(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsTokenRevoked(string token, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsTokenRevokedForUser(Guid userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}