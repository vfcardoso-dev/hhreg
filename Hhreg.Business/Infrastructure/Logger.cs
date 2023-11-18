using Spectre.Console;
using Spectre.Console.Rendering;

namespace Hhreg.Business.Infrastructure;

public interface ILogger
{
    Style DefaultRowStyle { get; }

    void WriteLine(string text);
    void WriteLine(FormattableString text);
    void Write(string text);
    void Write(FormattableString text);
    void Write(IRenderable renderable);
    void WriteTable(string[] headers, IEnumerable<string[]> rows);
    void WriteTable(string[] headers, IEnumerable<Text[]> rows);
    void WriteFilePath(string header, string filePath);
}

public class Logger : ILogger
{
    private static readonly Color HEADER_FG_COLOR = Color.Blue;
    private static readonly Color HEADER_BG_COLOR = Color.Black;
    private static readonly Color ROW_FG_COLOR = Color.White;
    private static readonly Color ROW_BG_COLOR = Color.Black;
    private static readonly Style _headerStyle = new(HEADER_FG_COLOR, HEADER_BG_COLOR);
    private static readonly Style _rowStyle = new(ROW_FG_COLOR, ROW_BG_COLOR);

    public Style DefaultRowStyle => _rowStyle;

    public void WriteLine(string text) => AnsiConsole.MarkupLine(text);
    public void WriteLine(FormattableString text) => AnsiConsole.MarkupLineInterpolated(text);
    public void Write(string text) => AnsiConsole.Markup(text);
    public void Write(FormattableString text) => AnsiConsole.MarkupInterpolated(text);
    public void Write(IRenderable renderable) => AnsiConsole.Write(renderable);

    public void WriteTable(string[] headers, IEnumerable<string[]> rows)
    {
        var table = new Table();
        var columns = headers.Select(header => new TableColumn(new Text(header, _headerStyle))).ToArray();
        var renderableRows = rows.Select(row => row.Select(cell => new Text(cell, _rowStyle)));

        table.AddColumns(columns);

        foreach (var renderableRow in renderableRows)
        {
            table.AddRow(renderableRow);
        }

        Write(table);
    }

    public void WriteTable(string[] headers, IEnumerable<Text[]> rows)
    {
        var table = new Table();
        var columns = headers.Select(header => new TableColumn(new Text(header, _headerStyle))).ToArray();

        table.AddColumns(columns);

        foreach (var renderableRow in rows)
        {
            table.AddRow(renderableRow);
        }

        Write(table);
    }

    public void WriteFilePath(string header, string filePath)
    {
        var table = new Table();
        table.AddColumns(new TableColumn(new Text(header, _headerStyle)));
        table.AddRow(new TextPath[] { new TextPath(filePath) });
        Write(table);
    }
}