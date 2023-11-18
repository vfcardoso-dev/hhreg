using Hhreg.Business.Commands;
using Hhreg.Business.Interceptors;
using Spectre.Console.Cli;

namespace Hhreg.Configs;

public interface ICommandsConfigurer
{
    void Configure(IConfigurator config);
}

public class CommandsConfigurer : ICommandsConfigurer
{
    private readonly IEnsureInitInterceptor _ensureInitInterceptor;

    public CommandsConfigurer(
        IEnsureInitInterceptor ensureInitInterceptor)
    {
        _ensureInitInterceptor = ensureInitInterceptor;
    }

    public void Configure(IConfigurator cmd)
    {
        cmd.PropagateExceptions();
        cmd.SetApplicationName("hhreg");
        cmd.CaseSensitivity(CaseSensitivity.None);
        cmd.ValidateExamples();

        cmd.SetInterceptor(_ensureInitInterceptor);

        cmd.AddCommand<InitCommand>("init")
            .WithDescription("Inicializa as configurações do CLI.")
            .WithExample(new string[] { "init", "--initial-balance", "-0:40", "--workday", "8:00", "--start-calculations-at", "01/12/2022" })
            .WithExample(new string[] { "init", "-m", "Minutes", "-b", "-20", "-w", "480", "-s", "01/12/2022" });

        cmd.AddCommand<UpdateCommand>("update")
            .WithDescription("Atualiza o app se houver uma nova versão disponível.");

        cmd.AddBranch("config", config =>
        {
            config.SetDescription("Gerencia as configurações do CLI.");

            config.AddCommand<ConfigShowCommand>("show")
                .WithDescription("Exibe as configurações atuais.");
            config.AddCommand<ConfigDatabaseCommand>("database")
                .WithDescription("Imprime o local atual do arquivo do banco de dados.");
            config.AddCommand<ConfigEditCommand>("edit")
                .WithDescription("Altera as configurações atuais.")
                .WithExample(new string[] { "config", "edit", "--initial-balance", "1:20", "--start-calculations-at", "01/04/2019" })
                .WithExample(new string[] { "config", "edit", "-m", "Minutes", "-b", "10" });
        });

        cmd.AddBranch("entry", entry =>
        {
            entry.SetDescription("Gerencia as entradas no registro.");

            entry.AddCommand<EntryNewCommand>("new")
                .WithDescription("Registra novas entradas.")
                .WithExample(new string[] { "entry", "new", "--day", "23/02/2023", "08:23", "12:01", "13:44", "19:37" })
                .WithExample(new string[] { "entry", "new", "-d", "12/01/2023", "-y", "Sick", "-j", "\"This is a justification\"" });

            entry.AddCommand<EntryOverrideCommand>("override")
                .WithDescription("Sobrescreve entradas no registro.")
                .WithExample(new string[] { "entry", "override", "--day", "23/02/2023", "08:23", "12:01", "13:44", "19:37" })
                .WithExample(new string[] { "entry", "override", "-d", "12/01/2023", "-y", "Sick", "-j", "\"This is a justification\"" });

            entry.AddCommand<EntryNowCommand>("now")
                .WithDescription("Escreve uma nova entrada no registro agora.");
        });

        cmd.AddBranch("report", report =>
        {
            report.SetDescription("Consolida e exibe as entradas no registro.");

            report.AddCommand<ReportSimulateCommand>("simulate")
                .WithDescription("Simula marcações para obter saldo acumulado estimado do banco de horas.")
                .WithExample(new string[] { "report", "simulate" })
                .WithExample(new string[] { "report", "simulate", "18:30" })
                .WithExample(new string[] { "report", "simulate", "-d", "01/11/2023", "18:30" });
            report.AddCommand<ReportBalanceCommand>("balance")
                .WithDescription("Exibe o saldo acumulado do banco de horas.")
                .WithExample(new string[] { "report", "balance" })
                .WithExample(new string[] { "report", "balance", "--tail", "30" });
            report.AddCommand<ReportMyDrakeCommand>("mydrake")
                .WithDescription("Exporta as entradas do registro para inclusão em lote no MyDrake.")
                .WithExample(new string[] { "report", "mydrake", "01/07/2023" })
                .WithExample(new string[] { "report", "mydrake", "01/03/2023", "31/03/2023" });
        });
    }
}