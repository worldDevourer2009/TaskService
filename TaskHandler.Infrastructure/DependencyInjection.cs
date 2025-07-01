using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskHandler.Domain.Repositories;
using TaskHandler.Infrastructure.Configurations;
using TaskHandler.Infrastructure.Persistence;
using TaskHandler.Infrastructure.Repositories;

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
    }
}