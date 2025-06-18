using Microsoft.EntityFrameworkCore;

namespace TaskHandler.Infrastructure.Extensions;

public static class DbSetExtensions
{
    public static bool TryAdd<T>(this DbSet<T> list, T value) where T : class
    {
        try
        {
            list.Add(value);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
} 