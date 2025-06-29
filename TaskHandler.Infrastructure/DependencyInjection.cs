using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        
        services.AddScoped<IDataSeeder, DataSeeder>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        
        BindEmailService(services, configuration);

        return services;
    }

    private static void BindEmailService(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IEmailSender, SmtpEmailSender>();
    }
}