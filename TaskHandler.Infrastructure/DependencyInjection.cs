using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        
        BindDb(services);
        BindTaskServices(services);

        BindEmailService(services, configuration);

        return services;
    }

    private static void BindTaskServices(IServiceCollection services)
    {
        services.AddScoped<ITaskGroupService, TaskGroupService>();
        services.AddScoped<ITaskService, TaskService>();
    }

    private static void BindDb(IServiceCollection services)
    {
        services.AddScoped<IDataSeeder, DataSeeder>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<ITaskGroupRepository, TaskGroupRepository>();
    }

    private static void BindEmailService(IServiceCollection services, IConfiguration configuration)
    {
    }
}