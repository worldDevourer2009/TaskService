namespace TaskHandler.Api.Middleware;

public static class TokenRefreshMiddlewareExtensions
{
    public static IApplicationBuilder UseTokenRefresh(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TokenRefreshMiddleware>();
    }
}