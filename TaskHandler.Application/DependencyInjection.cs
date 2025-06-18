using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace TaskHandler.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // MediatR для .NET 9
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });
        
        // AutoMapper (если планируете использовать)
        // services.AddAutoMapper(Assembly.GetExecutingAssembly());
        
       // services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}