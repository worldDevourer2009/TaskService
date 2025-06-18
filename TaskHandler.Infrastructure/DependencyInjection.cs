using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        return services;
    }
}