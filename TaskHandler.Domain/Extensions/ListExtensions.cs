namespace TaskHandler.Domain.Extensions;

public static class ListExtensions
{
    public static bool TryAdd<T>(this List<T> list, T value)
    {
        if (value == null)
        {
            return false;
        }

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