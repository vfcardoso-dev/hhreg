using hhreg.business;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace hhreg.tests;

public class LoggerStub : ILogger
{
    public IList<string> Lines { get; private set; } = new List<string>(); 
    public IList<string> Headers { get; private set; } = new List<string>(); 
    public IList<string[]> Rows { get; private set; } = new List<string[]>();
    public IList<string> MethodHits { get; private set; } = new List<string>();

    public Style DefaultRowStyle => new();

    public void Write(string text)
    {
        Lines.Add(text);
        MethodHits.Add("Write");
    }

    public void Write(FormattableString text)
    {
        Lines.Add(text.ToString());
        MethodHits.Add("Write");
    }

    public void Write(IRenderable renderable)
    {
        Lines.Add(renderable.ToString()!);
        MethodHits.Add("Write");
    }

    public void WriteFilePath(string header, string filePath)
    {
        Headers.Add(header);
        Rows.Add(new string[]{filePath});
        MethodHits.Add("WriteFilePath");
    }

    public void WriteLine(string text)
    {
        Lines.Add(text);
        MethodHits.Add("WriteLine");
    }

    public void WriteLine(FormattableString text)
    {
        Lines.Add(text.ToString());
        MethodHits.Add("WriteLine");
    }

    public void WriteTable(string[] headers, IEnumerable<string[]> rows)
    {
        foreach(var header in headers)
        {
            Headers.Add(header);
        }

        foreach(var row in rows)
        {
            Rows.Add(row);
        }

        MethodHits.Add("WriteTable");
    }

    public void WriteTable(string[] headers, IEnumerable<Text[]> rows)
    {
        foreach(var header in headers)
        {
            Headers.Add(header);
        }

        foreach(var row in rows)
        {
            Rows.Add(row.Select(x => x.ToString()).ToArray()!);
        }

        MethodHits.Add("WriteTable");
    }
}