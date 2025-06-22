using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TaskHandler.Domain.Entities;
using TaskHandler.Domain.Repositories;
using TaskHandler.Domain.Services;
using TaskHandler.Domain.ValueObjects;
using TaskHandler.Infrastructure.Services;

namespace TaskHandler.Tests.Users;

public class UserAuthTest
{
    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<IRedisService> _redisServiceMock;
    private Mock<IRevokedRefreshTokenRepository> _revokedRefreshTokenRepositoryMock;
    private Mock<IConfiguration> _configurationMock;
    private Mock<ILogger<TokenService>> _loggerMock;
    private TokenService _tokenService;

    [SetUp]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _redisServiceMock = new Mock<IRedisService>();
        _revokedRefreshTokenRepositoryMock = new Mock<IRevokedRefreshTokenRepository>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<TokenService>>();
        
        _configurationMock.Setup(x => x["JwtSettings:Key"]).Returns("MIHcAgEBBEIArHzR+NXhsA3b4HnodLD3IfScKCFl5NI04oNRInWV0pvgeO1f8nyFNDuqvtPOsMtC1vSi1bSAUqZMrCa8IrpauJ+gBwYFK4EEACOhgYkDgYYABAB/Iu9j8w4AOklu7oNJAArH8MaEVwKDCdM0oTXiUL6TVLx6cWxVjwXP0/N+EMvBzd5BlzsUl1aGSyCUiBlDYTEeqgAbEMrebDOtBTHWHHPImjz/+4Y88sEzPTAD+pYP04Yy9VVxpqb03REsqcUkiwpxaAEQNWh688TkuSJQRSVkRIzxlg==");
        _configurationMock.Setup(x => x["JwtSettings:Issuer"]).Returns("test_issuer");
        _configurationMock.Setup(x => x["JwtSettings:Audience"]).Returns("test_audience");
        _configurationMock.Setup(x => x["JwtSettings:AccessTokenLifetimeMinutes"]).Returns("60");
        _configurationMock.Setup(x => x["JwtSettings:RefreshTokenLifetimeDays"]).Returns("7");
        
        _tokenService = new TokenService(_userRepositoryMock.Object, _redisServiceMock.Object,
            _revokedRefreshTokenRepositoryMock.Object, _configurationMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task GenerateTokenPair_ValidUser_ReturnsValidTokenPair()
    {
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

        var (accessToken, refreshToken, rawRefreshToken) = await _tokenService.GenerateTokenPair(userId);
        
        Assert.Multiple(() =>
        {
            Assert.That(accessToken, Is.Not.Null);
            Assert.That(accessToken.TokenHash, Is.Not.Null);
            Assert.That(refreshToken, Is.Not.Null);
            Assert.That(refreshToken.TokenHash, Is.Not.Null);
            Assert.That(rawRefreshToken, Is.Not.Null);
        });
        
        _redisServiceMock.Verify(x => x.SetAsync(
            It.Is<string>(key => key.StartsWith("refresh:")),
            userId.ToString(),
            null
        ), Times.Once);
    } 
    
    [Test]
    public async Task ValidateToken_ValidToken_ReturnsTrue()
    {
        var userId = Guid.NewGuid();
        
        var testUser = User.Create(
            Email.Create("helloWorld@test.com"), 
            Password.Create("123Db998812"), 
            "New User");
        
        testUser.UpdateLastLogin();

        _userRepositoryMock.Setup(x => x.GetById(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testUser);
        
        var accessToken = await _tokenService.GenerateAccessToken(userId);
        
        Assert.That(accessToken, Is.Not.Null);
        
        Assert.That(accessToken.TokenHash, Is.Not.Null);
        
        var isValid = await _tokenService.ValidateToken(accessToken.TokenHash);

        // Assert
        Assert.That(isValid, Is.True);
    }
    
    [Test]
    public async Task IsRefreshTokenValid_ValidToken_ReturnsTrue()
    {
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
        Assert.That(isValid, Is.True);
    }
}