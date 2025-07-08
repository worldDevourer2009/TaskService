using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using TaskHandler.Application.Configurations;

namespace TaskHandler.Api.Middleware;

public class PublicKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AuthSettings _authSettings;
    private readonly JwtSettings _jwtSettings;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PublicKeyMiddleware> _logger;

    private static RSA? _cachedPublicKey;
    private static DateTime? _lastCheck;
    private static TimeSpan _checkInterval = TimeSpan.FromMinutes(30);
    private static object _lock = new();


    public PublicKeyMiddleware(RequestDelegate next, 
        IOptions<AuthSettings> authSettings, IOptions<JwtSettings> jwtSettings,
        IServiceProvider serviceProvider, ILogger<PublicKeyMiddleware> logger)
    {
        _next = next;
        _authSettings = authSettings.Value;
        _jwtSettings = jwtSettings.Value;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldCheckPublicKey())
        {
            await UpdatePublicKey();
        }

        await _next(context);
    }

    public bool ShouldCheckPublicKey()
    {
        return DateTime.UtcNow - _lastCheck > _checkInterval || _cachedPublicKey == null;
    }

    public async Task UpdatePublicKey()
    {
        lock (_lock)
        {
            if (!ShouldCheckPublicKey())
            {
                return;
            }

            try
            {
                var factory = _serviceProvider.GetRequiredService<IHttpClientFactory>();
                var client = factory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(10);
                
                var publicKey = client
                    .GetStringAsync($"{_authSettings.BaseUrl}/.well-known/public-key.pem")
                    .GetAwaiter()
                    .GetResult();
                
                var rsa = RSA.Create();
                rsa.ImportFromPem(publicKey);
                
                _cachedPublicKey = rsa;
                _lastCheck = DateTime.UtcNow;
                
                _logger.LogInformation("Public key updated");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while updating public key");

                try
                {
                    var fallbackKey = _jwtSettings.FallbackKey ??
                                      throw new Exception("Fallback key not found");

                    var rsa = RSA.Create();
                    rsa.ImportFromPem(fallbackKey);

                    _cachedPublicKey = rsa;
                    _lastCheck = DateTime.UtcNow;
                }
                catch (Exception e1)
                {
                    _logger.LogError(e1, "Error while updating public key");
                    throw;
                }
            }
        }
    }

    public static RSA GetPublicKey()
    {
        return _cachedPublicKey ?? throw new Exception("Public key not found");
    }
}