using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TaskHandler.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=taskhandler;Username=taskhandler_user;Password=taskhandler_password;Include Error Detail=true",
            b => b.MigrationsAssembly("TaskHandler.Infrastructure"));
        
        return new AppDbContext(optionsBuilder.Options);
    }
}