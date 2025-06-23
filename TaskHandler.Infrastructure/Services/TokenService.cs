using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using TaskHandler.Application.Interfaces;
using TaskHandler.Domain.Repositories;
using TaskHandler.Domain.Services;
using TaskHandler.Domain.ValueObjects;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace TaskHandler.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IUserRepository _userRepository;
    private readonly IRedisService _redisService;
    private readonly IRevokedRefreshTokenRepository _revokedRefreshTokenRepository;
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenService> _logger;
    private readonly string? _secretKey;
    private readonly string? _issuer;

    public TokenService(IUserRepository userRepository, IRedisService redisService, IRevokedRefreshTokenRepository refreshTokenRepository, IConfiguration configuration,
        ILogger<TokenService> logger)
    {
        _userRepository = userRepository;
        _revokedRefreshTokenRepository = refreshTokenRepository;
        _redisService = redisService;
        
        _configuration = configuration;
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

    public async Task<AccessToken> GenerateAccessToken(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetById(userId, cancellationToken);

        if (user == null)
        {
            throw new Exception("User not found, can't create token");
        }

        if (user.LastLogin == null || user.Email == null || user.Name == null)
        {
            throw new Exception("User has invalid data");
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim(JwtRegisteredClaimNames.AuthTime,
                new DateTimeOffset(user.LastLogin.Value).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
        };

        if (_secretKey == null || _issuer == null)
        {
            throw new Exception("JwtSettings:Key or JwtSettings:Issuer is required");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(_issuer, audience: _configuration["JwtSettings:Audience"], claims,
            expires: now.AddMinutes(60),
            signingCredentials: credentials,
            notBefore: now);

        var timeSpan = TimeSpan.FromMinutes(int.Parse(_configuration["JwtSettings:AccessTokenLifetimeMinutes"] ?? "60"));
        
        var tokenString = _jwtSecurityTokenHandler.WriteToken(token);
        var accessToken = AccessToken.Create(tokenString, timeSpan);
        return accessToken;
    }

    private async Task<(RefreshToken hashedToken, string rawToken)> GenerateRefreshToken(Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetById(userId, cancellationToken);

        if (user == null)
        {
            throw new Exception("User not found, can't create token");
        }

        if (user.LastLogin == null || user.Email == null || user.Name == null)
        {
            throw new Exception("User has invalid data");
        }

        var randomBytes = RandomNumberGenerator.GetBytes(32);
        var opaqueToken = WebEncoders.Base64UrlEncode(randomBytes);
        var tokenHash = SHA256.HashData(Encoding.UTF8.GetBytes(opaqueToken));
        
        var timeSpan = TimeSpan.FromDays(int.Parse(_configuration["JwtSettings:RefreshTokenLifetimeDays"] ?? "7"));
        
        var refreshToken = RefreshToken.Create(Convert.ToHexString(tokenHash), timeSpan);
        return (refreshToken, opaqueToken);
    }

    private async Task<bool> AddRefreshToken(RefreshToken token, Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (token.TokenHash != null)
        {
            var key = RefreshKey(token.TokenHash);

            if (!await _redisService.SetAsync(key, userId.ToString()))
            {
                throw new Exception("Can't add refresh token to redis");
            }
        }

        return true;
    }

    public async Task<(AccessToken accessToken, RefreshToken refreshToken, string rawRefreshToken)> GenerateTokenPair(Guid userId,
        CancellationToken cancellationToken = default)
    {
        var accessToken = await GenerateAccessToken(userId, cancellationToken);
        var (refreshToken, rawToken) = await GenerateRefreshToken(userId, cancellationToken);

        try
        {
            await AddRefreshToken(refreshToken, userId, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Can't add refresh token to redis");
        }
        
        return (accessToken, refreshToken, rawToken);
    }

    public async Task<bool> ValidateToken(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentNullException("Token is required" + nameof(token));
        }

        if (_secretKey == null || _issuer == null)
        {
            throw new Exception("JwtSettings:Key or JwtSettings:Issuer is required");
        }

        if (await IsAccessTokenRevoked(token))
        {
            return false;
        }

        if (!_jwtSecurityTokenHandler.CanReadToken(token))
        {
            return false;
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
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            var validatedToken =
                _jwtSecurityTokenHandler.ValidateToken(token, validationParameters, out var securityToken);

            if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            return true;
        }
        catch (SecurityTokenException)
        {
            return false;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Token validation error");
            return false;
        }
    }

    public async Task<bool> RevokeRefreshToken(string rawRefreshToken)
    {
        if (string.IsNullOrWhiteSpace(rawRefreshToken))
        {
            throw new ArgumentNullException(nameof(rawRefreshToken), "Token is required");
        }

        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawRefreshToken)));

        if (!await _redisService.RemoveAsync(RefreshKey(tokenHash)))
        {
            _logger.LogWarning("Failed to remove refresh token from Redis");
        }

        try
        {
            await _revokedRefreshTokenRepository.RevokeToken(tokenHash);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding refresh token to revoked tokens");
            return false;
        }
    }

    public async Task<bool> RevokeAccessToken(string jwtString)
    {
        if (string.IsNullOrWhiteSpace(jwtString))
        {
            throw new ArgumentNullException(nameof(jwtString));
        }

        var jwt = _jwtSecurityTokenHandler.ReadJwtToken(jwtString);
        var jti = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value
                  ?? throw new Exception("Missing JTI");
        
        var ttl = jwt.ValidTo - DateTime.UtcNow;
        if (ttl <= TimeSpan.Zero)
        {
            return false;
        }
        
        await _redisService.SetAsync($"revoked:{jti}", "1", ttl);
        return true;
    }

    public async Task<bool> IsAccessTokenRevoked(string jwtString)
    {
        if (string.IsNullOrWhiteSpace(jwtString))
        {
            return false;
        }

        try
        {
            var jwtToken = _jwtSecurityTokenHandler.ReadJwtToken(jwtString);
            var jti = jwtToken.Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Jti).Value;
            var userIdClaim = jwtToken.Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value;
            var authTimeClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.AuthTime)?.Value;

            if (string.IsNullOrEmpty(jti) || string.IsNullOrEmpty(userIdClaim))
            {
                return true;
            }
            
            if (await _redisService.KeyExistsAsync(RevokedKey(jti)))
            {
                return true;
            }
            
            var userRevokeKey = $"user_revoked:{userIdClaim}";
            var userRevokeTimestamp = await _redisService.GetAsync(userRevokeKey);
        
            if (!string.IsNullOrEmpty(userRevokeTimestamp) && !string.IsNullOrEmpty(authTimeClaim))
            {
                if (long.TryParse(userRevokeTimestamp, out var revokeTime) && 
                    long.TryParse(authTimeClaim, out var authTime))
                {
                    return authTime < revokeTime;
                }
            }

            return false;
        }
        catch
        {
            return true;
        }
    }

    public async Task<bool> IsRefreshTokenValid(string rawRefreshToken)
    {
        if (string.IsNullOrWhiteSpace(rawRefreshToken))
        {
            return false;
        }

        try
        {
            var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawRefreshToken)));

            if (!await _redisService.KeyExistsAsync(RefreshKey(tokenHash)))
            {
                return false;
            }

            return !await _revokedRefreshTokenRepository.IsRevoked(tokenHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating refresh token");
            return false;
        }
    }

    public async Task<(AccessToken newAccessToken, RefreshToken newRefreshToken, string rawRefreshToken)> RefreshTokens(
        string rawRefreshToken, CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(rawRefreshToken))
        {
            throw new ArgumentNullException(nameof(rawRefreshToken), "Token is required");
        }

        if (!await IsRefreshTokenValid(rawRefreshToken))
        {
            throw new SecurityTokenException("Refresh token is not revoked");
        }

        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawRefreshToken)));
        var userId = await _redisService.GetAsync(RefreshKey(tokenHash));

        if (userId == null || !Guid.TryParse(userId, out var id))
        {
            throw new Exception("Refresh token is not valid");
        }

        await RevokeRefreshToken(rawRefreshToken);
        
        var (newTokenAccess, newTokenRefresh, newRawRefreshToken) = await GenerateTokenPair(id, cancellationToken);
        return (newTokenAccess, newTokenRefresh, newRawRefreshToken);
    }

    public async Task<bool> RevokeAllTokensForUser(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetById(userId, cancellationToken);
        
        if (user == null)
        {
            return false;
        }
        
        try
        {
            var refreshKeyPattern = RefreshKey("*");
            var refreshKeys = await _redisService.GetKeysByPatternAsync(refreshKeyPattern);
            
            foreach (var key in refreshKeys)
            {
                var storedUserId = await _redisService.GetAsync(key);
                if (storedUserId == userId.ToString())
                {
                    await _redisService.RemoveAsync(key);
                    
                    var hash = key.Replace("refresh:", "");
                    await _revokedRefreshTokenRepository.RevokeToken(hash);
                }
            }
            
            var userRevokeKey = $"user_revoked:{userId}";
            var revokeTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            
            var accessTokenLifetime = TimeSpan.FromMinutes(int.Parse(_configuration["JwtSettings:AccessTokenLifetimeMinutes"] ?? "60"));
            await _redisService.SetAsync(userRevokeKey, revokeTimestamp, accessTokenLifetime);
            
            _logger.LogInformation("All tokens revoked for user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking all tokens for user {UserId}", userId);
            return false;
        }
    }
    
    private static string RefreshKey(string hash) => $"refresh:{hash}";
    private static string AccessKey(string hash)  => $"access:{hash}";
    private static string RevokedKey(string jti)  => $"revoked:{jti}";
}