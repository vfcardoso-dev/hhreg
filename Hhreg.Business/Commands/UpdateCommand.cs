using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using Spectre.Console;
using Spectre.Console.Cli;
using System.IO.Compression;
using Hhreg.Business.Exceptions;
using Hhreg.Business.Infrastructure;

namespace Hhreg.Business.Commands;

public sealed class UpdateCommand : Command<UpdateCommand.Settings>
{
    private readonly ISettingsService _appSettings;
    private readonly ILogger _logger;

    private const string RepositoryUrl = "https://github.com/vfcardoso-dev/hhreg";

    public UpdateCommand(ISettingsService appSettings, ILogger logger)
    {
        _appSettings = appSettings;
        _logger = logger;
    }

    public sealed class Settings : CommandSettings { }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        return AnsiConsole.Status()
            .Spinner(Spinner.Known.Star2)
            .Start("Atualizando hhreg...", ctx =>
            {
                AnsiConsole.MarkupLine("[blue]Processo de atualização iniciado...[/]");

                AnsiConsole.WriteLine();
                var currentVersion = Assembly.GetEntryAssembly()?.GetName().Version!.ToString(3);
                AnsiConsole.MarkupLineInterpolated($"Versão instalada é [green]v{currentVersion}[/]");

                var lastVersion = GetLastVersionNumber();
                AnsiConsole.MarkupLineInterpolated($"Última versão disponível no repositório é [green]v{lastVersion}[/]");

                if (currentVersion == lastVersion)
                {
                    AnsiConsole.Markup("[green bold]Hhreg já está atualizado com a última versão. =)[/]");
                    return 0;
                }

                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("Existe uma [u]nova versão[/]! Vamos [green]atualizar[/]!");

                AnsiConsole.WriteLine();
                var platform = GetPlatform();
                DownloadArtifact(lastVersion, platform);

                AnsiConsole.MarkupLine("Extraindo arquivos...");
                UnzippingFiles(platform);

                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("Executando atualizador...");

                RunUpdateCommand(platform);

                Thread.Sleep(6000);

                AnsiConsole.MarkupLine("Removendo arquivos temporários...");
                Directory.Delete(GetAppFolder("updating"), true);

                AnsiConsole.MarkupLine("[green bold]Atualização completa![/]");

                ctx.Status("[green bold]Atualização completa![/]");

                Thread.Sleep(1000);

                return 0;
            });
    }

    private void RunUpdateCommand(string platform)
    {
        var origin = GetAppFolder("updating");
        var destination = GetAppFolder();
        string cmdStr;

        if (platform.StartsWith("win"))
        {
            cmdStr = $"/c start cmd.exe /c copy /b /y {origin} {destination}";
        }
        else
        {
            cmdStr = $"-c bash -c cp -rf {origin} {destination}";
        }

        System.Diagnostics.Process.Start("cmd.exe", cmdStr);
    }

    private string GetAppFolder(string? pathToAppend = null)
    {
        var path = Path.GetDirectoryName(AppContext.BaseDirectory)!;
        return pathToAppend == null ? path : Path.Combine(path, pathToAppend);
    }

    private void DownloadArtifact(string version, string platform)
    {
        var extension = platform.StartsWith("win") ? "zip" : "tar.gz";
        var artifactUrl = $"{RepositoryUrl}/releases/download/v{version}/hhreg-v{version}-{platform}.{extension}";
        var destinationFolder = GetAppFolder("updating");

        _logger.WriteLine($"Fazendo download do artefato de [green]{artifactUrl}[/]...");

        if (!Directory.Exists(destinationFolder)) Directory.CreateDirectory(destinationFolder);
        var destinationFile = Path.Combine(destinationFolder, $"artifact.{extension}");

        using var httpClient = new HttpClient(new HttpClientHandler { AllowAutoRedirect = true });
        using var response = httpClient.GetStreamAsync(artifactUrl).Result;
        using var fileStream = new FileStream(destinationFile, FileMode.Create);

        response.CopyTo(fileStream);
    }

    private void UnzippingFiles(string platform)
    {
        var extension = platform.StartsWith("win") ? "zip" : "tar.gz";
        var fileToExtract = Path.Combine(GetAppFolder("updating"), $"artifact.{extension}");
        var destination = GetAppFolder("updating");

        if (File.Exists(fileToExtract))
        {
            ZipFile.ExtractToDirectory(fileToExtract, destination);
            File.Delete(fileToExtract);
        }
        else
        {
            throw new HhregException(HhregMessages.UpdateArtifactNotFound);
        }
    }

    private static string GetPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return "win-x64";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return "linux-x64";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return "osx-x64";
        else throw new HhregException(HhregMessages.UnknownPlatform);
    }

    private static string GetLastVersionNumber()
    {

        var url = $"{RepositoryUrl}/releases/latest";

        using HttpClient httpClient = new(new HttpClientHandler { AllowAutoRedirect = false });
        var response = httpClient.GetAsync(url).Result;

        if (response == null) throw new HhregException(HhregMessages.LastestReleaseUnavailable);

        var version = response.Headers.Location!.ToString().Split('/').Last()!;

        return version[1..];
    }
}