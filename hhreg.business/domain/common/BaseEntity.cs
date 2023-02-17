using Spectre.Console;
using Spectre.Console.Rendering;

public abstract class BaseEntity<T>
{
    public virtual TableColumn[] CreateHeaders() 
    {
        var headers = new List<TableColumn>();
        headers.AddRange(typeof(T).GetProperties().Select(x => 
            new TableColumn(new Text(x.Name, new Style(Color.Green, Color.Black)))));
        return headers.ToArray();
    }

    public virtual IRenderable[] CreateRenderableRow()
    {
        var row = new List<Text>();

        row.AddRange(typeof(T).GetProperties()
            .Select(x => 
                new Text(x.GetValue(this)?.ToString() ?? "")));

        return row.ToArray();
    }
}