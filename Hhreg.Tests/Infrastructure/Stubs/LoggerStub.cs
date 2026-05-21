using System.Reflection;
using Hhreg.Business.Infrastructure;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Hhreg.Tests.Infrastructure.Stubs;

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
        Rows.Add(new string[] { filePath });
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
        foreach (var header in headers)
        {
            Headers.Add(header);
        }

        foreach (var row in rows)
        {
            Rows.Add(row);
        }

        MethodHits.Add("WriteTable");
    }

    public void WriteTable(string[] headers, IEnumerable<Text[]> rows)
    {
        foreach (var header in headers)
        {
            Headers.Add(header);
        }

        foreach (var row in rows)
        {
            Rows.Add(row.Select(ExtractText).ToArray()!);
        }

        MethodHits.Add("WriteTable");
    }

    private static readonly FieldInfo? ParagraphField = typeof(Text)
        .GetField("_paragraph", BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly FieldInfo? LinesField = ParagraphField?.FieldType
        .GetField("_lines", BindingFlags.NonPublic | BindingFlags.Instance);

    private static string ExtractText(Text text)
    {
        var paragraph = ParagraphField?.GetValue(text);
        if (paragraph == null || LinesField == null) return text.ToString() ?? "";

        if (LinesField.GetValue(paragraph) is not System.Collections.IEnumerable lines) return "";

        var builder = new System.Text.StringBuilder();
        foreach (var line in lines)
        {
            if (line is not System.Collections.IEnumerable segments) continue;
            foreach (var segment in segments)
            {
                var textProperty = segment.GetType().GetProperty("Text");
                builder.Append(textProperty?.GetValue(segment) as string);
            }
        }
        return builder.ToString();
    }

    public void Clear()
    {
        Lines.Clear();
        Headers.Clear();
        Rows.Clear();
        MethodHits.Clear();
    }
}