using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using TaskHandler.Application.Interfaces;
using TaskHandler.Domain.Repositories;
using TaskHandler.Domain.Services;
using TaskHandler.Infrastructure.Configurations;
using TaskHandler.Infrastructure.Persistence;
using TaskHandler.Infrastructure.Repositories;
using TaskHandler.Infrastructure.Services;

namespace TaskHandler.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDataSeeder, DataSeeder>();
        services.AddScoped<IUserPasswordService, UserPasswordService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IUserLoginService, UserLoginService>();
        services.AddScoped<IUserSignUpService, UserSignUpService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IRevokedRefreshTokenRepository, RevokedRefreshTokenRepository>();
        services.AddScoped<IUserLogoutService, UserLogoutService>();
        
        services.Configure<EmailSettings>(options => configuration.GetSection("EmailSettings"));
        services.Configure<JwtSettings>(options => configuration.GetSection("JwtSettings"));

        BindRedis(services, configuration);
        BindEmailService(services, configuration);

        return services;
    }

    private static void BindRedis(IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis") ?? configuration["RedisSettings:ConnectionString"];
        
        if (redisConnectionString == null)
        {
            throw new Exception("Redis connection string is not set");
        }

        if (!redisConnectionString.Contains("abortConnect=false"))
        {
            redisConnectionString = redisConnectionString + ",abortConnect=false";
        }

        try
        {
            var redis = ConnectionMultiplexer.Connect(redisConnectionString);
            services.AddSingleton(redis);
            services.AddSingleton<IRedisService, RedisService>();
        }
        catch (Exception e)
        {
            throw new Exception("Can't connect to Redis", e);
        }
    }

    private static void BindEmailService(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IEmailSender>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<EmailSettings>>().Value;
            
            return new SmtpEmailSender(
                options.SmtpServer,
                options.SmtpPort,
                options.UsernameSmtp,
                options.PasswordSmtp,
                options.EnableSmtpSsl,
                options.FromSmtpName,
                options.FromSmtpDisplayName);
        });
    }
}