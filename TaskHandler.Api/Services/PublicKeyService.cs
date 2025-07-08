using Microsoft.IdentityModel.Tokens;
using TaskHandler.Api.Middleware;

namespace TaskHandler.Api.Services;

public interface IPublicKeyService
{
    RsaSecurityKey GetPublicKey();
    Task RefreshPublicKey();
}

public class PublicKeyService : IPublicKeyService
{
    public RsaSecurityKey GetPublicKey()
    {
        var rsa = PublicKeyMiddleware.GetPublicKey();
        return new RsaSecurityKey(rsa);
    }

    public async Task RefreshPublicKey()
    {
        await Task.CompletedTask;
    }
}