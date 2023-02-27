using Spectre.Console;

namespace hhreg.business;

public abstract class BaseEntity<T>
{
    public virtual string[] ExtractColumns() 
    {
        return typeof(T).GetProperties().Select(x => x.Name).ToArray();
    }

    public virtual string[] ExtractRow()
    {
        return typeof(T).GetProperties().Select(x => x.GetValue(this)?.ToString() ?? "").ToArray();
    }
}