using System.Net;
using TaskHandler.Application.Interfaces;
using TaskHandler.Domain.ValueObjects;

namespace TaskHandler.Api.Middleware;

public class TokenRefreshMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenRefreshMiddleware> _logger;

    public TokenRefreshMiddleware(RequestDelegate next, ILogger<TokenRefreshMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITokenService tokenService)
    {
        await _next(context);

        if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
        {
            await HandleTokenRefresh(context, tokenService);
        }
    }

    private async Task HandleTokenRefresh(HttpContext context, ITokenService tokenService)
    {
        _logger.LogInformation("Attempting token refresh");

        try
        {
            if (!context.Request.Cookies.TryGetValue("refresh_token", out var refreshToken))
            {
                _logger.LogInformation("No refresh token found in cookies");
                return;
            }

            if (!await tokenService.IsRefreshTokenValid(refreshToken))
            {
                _logger.LogInformation("Invalid refresh token");
                return;
            }

            var (newAccessToken, newRefreshToken, rawRefreshToken) =
                await tokenService.RefreshTokens(refreshToken);

            SetTokenCookies(context, newAccessToken, newRefreshToken, rawRefreshToken);

            var returnUrl = context.Request.Path + context.Request.QueryString;
            context.Response.Redirect(returnUrl);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Token refresh failed");
            RedirectionToLogin(context);
        }
    }

    private void SetTokenCookies(HttpContext context, AccessToken newAccessToken, RefreshToken newRefreshToken,
        string rawRefreshToken)
    {
        if (newAccessToken.TokenHash != null)
            context.Response.Cookies.Append("access_token", newAccessToken.TokenHash, new CookieOptions
            {
                HttpOnly = true,
                Expires = newAccessToken.ExpirationDate,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });

        context.Response.Cookies.Append("refresh_token", rawRefreshToken, new CookieOptions()
        {
            HttpOnly = true,
            Expires = newRefreshToken.ExpirationDate,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });
    }

    private void RedirectionToLogin(HttpContext context)
    {
        context.Response.Cookies.Delete("access_token");
        context.Response.Cookies.Delete("refresh_token");
        context.Response.Redirect("/login");
    }
}