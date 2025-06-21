using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using TaskHandler.Application.Interfaces;
using TaskHandler.Domain.Repositories;
using TaskHandler.Domain.Services;
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
        var smtpServer = configuration["EmailSettings:SmtpServer"];
        var smtpPortStr = configuration["EmailSettings:SmtpPort"];
        var smtpUser = configuration["EmailSettings:SmtpUsername"];
        var smtpPassword = configuration["EmailSettings:SmtpPassword"];
        var enableSslStr = configuration["EmailSettings:EnableSsl"];
        var smtpFrom = configuration["EmailSettings:FromEmail"];
        var smtpFromDisplayName = configuration["EmailSettings:FromName"];
        
        int smtpPort = 587;
        bool enableSsl = true;
        
        if (!string.IsNullOrEmpty(smtpPortStr) && int.TryParse(smtpPortStr, out int parsedPort))
        {
            smtpPort = parsedPort;
        }
    
        if (!string.IsNullOrEmpty(enableSslStr) && bool.TryParse(enableSslStr, out bool parsedSsl))
        {
            enableSsl = parsedSsl;
        }
    
        services.AddSingleton<IEmailSender>(sp => new SmtpEmailSender(
            smtpServer: smtpServer,
            smtpPort: smtpPort,
            smtpUser: smtpUser,
            smtpPassword: smtpPassword,
            smtpEnableSsl: enableSsl,
            smtpFrom: smtpFrom,
            smtpFromDisplayName: smtpFromDisplayName
        ));
    }
}