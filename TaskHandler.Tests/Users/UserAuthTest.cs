using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TaskHandler.Domain.Entities;
using TaskHandler.Domain.Repositories;
using TaskHandler.Domain.Services;
using TaskHandler.Domain.ValueObjects;
using TaskHandler.Infrastructure.Configurations;
using TaskHandler.Infrastructure.Services;
using Xunit;

namespace TaskHandler.Tests.Users;

public class UserAuthTest
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRedisService> _redisServiceMock;
    private readonly Mock<IRevokedRefreshTokenRepository> _revokedRefreshTokenRepositoryMock;
    private readonly Mock<ILogger<TokenService>> _loggerMock;
    private readonly TokenService _tokenService;

    public UserAuthTest()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _redisServiceMock = new Mock<IRedisService>();
        _revokedRefreshTokenRepositoryMock = new Mock<IRevokedRefreshTokenRepository>();
        _loggerMock = new Mock<ILogger<TokenService>>();

        var jwtSettings = new JwtSettings()
        {
            Key = "MIHcAgEBBEIB7Ciph8O5ZeuVT46lzj7cRyPxZaiGxh1pvqnHIwNC/gXd6fDFmTOo\ns8NQ+PSCQglLRpZv6rMp3j5FUsBcocT1OZmgBwYFK4EEACOhgYkDgYYABAFsumQX\njiC/meBVIjpI2aRUH1v2YjOdVOzyrZKiqCNHfG7H9RMraMW6OO78EZAfMgSvr5TW\nieLYX3L0wyh2117osQGBEzePMBDxBnNBxZqg6AtPiXNyfRe0/vuukjdMSfgoQ37s\nMf9YOgNT7YRxpQUrrC90tv98eU5BTdaq7qhPNvY4tg==",
            Issuer = "test_issuer",
            Audience = "test_audience",
            RefreshTokenLifetimeDays = 60,
            AccessTokenLifetimeMinutes = 7,
        };
        
        var options = Options.Create(jwtSettings);
        
        _tokenService = new TokenService(_userRepositoryMock.Object, _redisServiceMock.Object,
            _revokedRefreshTokenRepositoryMock.Object, options, _loggerMock.Object);
    }

    [Fact]
    public async Task GenerateTokenPair_ValidUser_ReturnsValidTokenPair()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var testUser = User.Create(
            Email.Create("helloWorld@test.com"), 
            Password.Create("123Db998812"), 
            "New User");
        
        testUser.UpdateLastLogin();
        
        _userRepositoryMock.Setup(x => 
            x.GetById(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testUser);
        
        _redisServiceMock.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), null))
            .ReturnsAsync(true);

        // Act
        var (accessToken, refreshToken, rawRefreshToken) = await _tokenService.GenerateTokenPair(userId);
        
        // Assert
        Assert.NotNull(accessToken);
        Assert.NotNull(accessToken.TokenHash);
        Assert.NotNull(refreshToken);
        Assert.NotNull(refreshToken.TokenHash);
        Assert.NotNull(rawRefreshToken);
        
        _redisServiceMock.Verify(x => x.SetAsync(
            It.Is<string>(key => key.StartsWith("refresh:")),
            userId.ToString(),
            null
        ), Times.Once);
    } 
    
    [Fact]
    public async Task ValidateToken_ValidToken_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        var testUser = User.Create(
            Email.Create("helloWorld@test.com"), 
            Password.Create("123Db998812"), 
            "New User");
        
        testUser.UpdateLastLogin();

        _userRepositoryMock.Setup(x => x.GetById(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testUser);
        
        // Act
        var accessToken = await _tokenService.GenerateAccessToken(userId);
        
        Assert.NotNull(accessToken);
        Assert.NotNull(accessToken.TokenHash);
        
        var isValid = await _tokenService.ValidateToken(accessToken.TokenHash);

        // Assert
        Assert.True(isValid);
    }
    
    [Fact]
    public async Task IsRefreshTokenValid_ValidToken_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        var testUser = User.Create(
            Email.Create("helloWorld@test.com"), 
            Password.Create("123Db998812"), 
            "New User");

        testUser.UpdateLastLogin();

        _userRepositoryMock.Setup(x => x.GetById(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testUser);
        
        _redisServiceMock.Setup(x => x.KeyExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
        
        _revokedRefreshTokenRepositoryMock.Setup(x => x.IsRevoked(It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var (_, refreshToken, rawRefreshToken) = await _tokenService.GenerateTokenPair(userId);
        var isValid = await _tokenService.IsRefreshTokenValid(rawRefreshToken);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public Task LogoutUser_And_Token_Are_Revoked()
    {
        //Arrange
        
        return Task.CompletedTask;
    }
}