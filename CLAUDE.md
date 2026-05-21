# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Visão geral

`hhreg` é uma CLI escrita em C# / .NET 6 para registro de marcações de ponto e cálculo de banco de horas. Os dados ficam no `%LocalAppData%\hhreg\` (SQLite `hhreg.db` + `settings.json`). A UX usa Spectre.Console (cores, tabelas, spinner). Mensagens, descrições e nomes de testes estão em Português.

## Build, run e testes (.NET 6 SDK)

Este projeto é SDK-style — use `dotnet` CLI, **não** o MSBuild apontado no CLAUDE.md global (aquele caminho é para .NET Framework).

```bash
dotnet build hhreg.sln                                  # compila tudo (Debug)
dotnet run --project Hhreg/Hhreg.csproj -- <comando>    # executa CLI em dev
dotnet test                                              # roda toda a suíte de testes
dotnet test --filter FullyQualifiedName~EntryNewCommandTests   # roda uma classe
dotnet test --filter "FullyQualifiedName~EntryNewCommandTests.deve_ser_capaz_de_lancar_entradas"   # roda um teste
dotnet watch run --project Hhreg/Hhreg.csproj           # watch (também há tasks no .vscode/tasks.json)
```

Importante:
- `Hhreg` e `Hhreg.Business` têm `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` e `<Nullable>enable</Nullable>`. Qualquer warning quebra o build.
- O publish é single-file self-contained (`PublishSingleFile=true`, `IncludeNativeLibrariesForSelfExtract=true`, `RuntimeIdentifier=win-x64`). O workflow `.github/workflows/ci.yaml` publica para `linux-x64`, `win-x64` e `osx-x64` em releases do GitHub.
- O `ImplicitUsings=enable` está ligado nos três projetos — vários `using` comuns não aparecem no topo dos arquivos.

## Arquitetura — o panorama

A entrada é `Hhreg/Program.cs`, que monta um `Host.CreateDefaultBuilder` para configurar DI e configuração, depois delega para `AppHost.Run(args)`, que envolve toda execução num `AnsiConsole.Status().Start("Calculando...", ...)` (exceto `--help`, `--version`, `update`).

`AppHost` faz três coisas, em ordem:
1. `IDatabaseEnsurer.Ensure()` — cria pasta + `hhreg.db` se não existir, garante schema das tabelas `DayEntry` / `TimeEntry`, e migra a tabela legada `Settings` para o arquivo `settings.json` se ainda existir no banco.
2. Cria um `CommandApp` (Spectre.Console.Cli) usando o `TypeRegistrar`, que adapta `IServiceCollection` (Microsoft DI) à abstração do Spectre.
3. Aplica `ICommandsConfigurer` (em `Hhreg/Configs/CommandsConfigurer.cs`) — esse é o arquivo central que define a árvore de comandos: `init`, `update`, `config (show|database|recalculate|edit)`, `entry (new|override|now)`, `report (simulate|balance|mydrake)`.

Antes de cada comando, `EnsureInitInterceptor` (registrado via `cmd.SetInterceptor`) checa se `settings.json` existe e está preenchido. Se não, joga `HhregException` — exceto quando o comando é `init`.

### Camadas (projeto `Hhreg.Business`)

- **Commands/** — uma classe por comando, sempre `Command<Settings>` ou `AsyncCommand<Settings>`. Cada comando tem uma `Settings : CommandSettings` aninhada com atributos `[CommandOption]`, `[CommandArgument]`, `[Description]` e um `override Validate()` que devolve `ValidationResult`. Comandos de relatório herdam de `ReportCommandBase<T>`, que oferece `CheckInvalidTimeEntries()` (alerta dias com número ímpar de marcações).
- **Domain/** — entidades `DayEntry` (1 por dia) e `TimeEntry` (N por dia), `DayType` enum (`Work, Weekend, Sick, Holiday, Vacation, Justified`), `Settings` (StartBalanceInMinutes, WorkDayInMinutes, EntryToleranceInMinutes, LastBalanceCutoff). `Domain/Dtos/` tem DTOs como `LegacySettings` e `MyDrakeEvent`. `Domain/ValueObjects/TimeInputMode.cs` define `Hours`/`Minutes`.
- **Repositories/TimeRepository.cs** — único repositório, faz toda a leitura/escrita de `DayEntry` e `TimeEntry`. Concentra o cálculo do saldo acumulado via SQL (`GetAccumulatedBalance`).
- **Infrastructure/** — `SettingsService` (lê/grava `settings.json` em `%LocalAppData%\hhreg\`), `UnitOfWork`/`UnitOfWorkContext` (wrapper sobre `SqliteConnection` + Dapper, expõe `Execute`/`Query`/`BulkExecute` com transação), `Logger` (fachada sobre `AnsiConsole` com markup e renderização de tabelas), `TypeRegistrar`/`TypeResolver` (ponte DI ↔ Spectre).
- **Interceptors/** — só o `EnsureInitInterceptor`.
- **Utilities/** — `Calculations.GetTotalMinutes` é o cálculo central: soma pares ordenados de marcações, aplica tolerância em torno do workday, e para `DayType != Work` retorna `workdayInMinutes` direto. `DateAndTimeOnlyExtensions`, `TimeExtensions`, `StringExtensions` e `SpectreConsoleUtils` (helpers de formatação para as tabelas dos relatórios).
- **Exceptions/** — `HhregException` e `HhregMessages` (strings constantes, todas em Português, usadas tanto para `ValidationResult.Error` quanto para mensagens de exception).

### Onde toca quando…

- Adicionando um comando novo: criar classe em `Hhreg.Business/Commands/`, registrar em `Hhreg/Configs/CommandsConfigurer.cs` (com `WithDescription` e `WithExample`), e adicionar testes em `Hhreg.Tests/Commands/`.
- Adicionando dependência injetada: registrar em `Hhreg/Configs/ServicesConfigurer.cs` (escopo: `AddScoped` para coisas que dependem do `UnitOfWork`/`DbConnection`, `AddSingleton` para serviços sem estado por requisição).
- Mudou um construtor com nova dependência: atualizar mocks (`Substitute.For<...>`) dos testes — vale o que está no CLAUDE.md global.

## Padrão de testes

`Hhreg.Tests/Infrastructure/UnitTestsBase.cs` é a base. NUnit + FluentAssertions + NSubstitute + AutoFixture. Cada teste:
1. Registra dependências mockadas via `AddSingleton<T>(instance)`.
2. Chama `CreateCommandApp(config => config.AddCommand<TheCommand>("name"))` — isso constrói um `CommandApp` isolado com `PropagateExceptions()`.
3. `app.Run(new[] { "name", ...args })` e asserta com FluentAssertions; falhas de validação se propagam como exceptions (em vez de `Console.Error`).

Builders em `Hhreg.Tests/Infrastructure/` (`DayEntryBuilder`, `TimeEntryBuilder`, `SettingsBuilder`) são customizações do `AutoFixture` registradas em `UnitTestsBase.CreateFixture()`. `LoggerStub` captura `Lines` e `MethodHits` para asserts em saída.

Convenção de nomes de teste: `deve_<comportamento>_<condição>` em snake_case Português.

## Convenções e armadilhas

- **Datas no SQLite** são guardadas como `TEXT` no formato `yyyy-MM-dd`. O domínio trabalha com `DateOnly`. Sempre converter antes de passar para Dapper (vide `TimeRepository.GetDayEntry`).
- **Cálculo de saldo** depende de `WorkDayInMinutes` e `LastBalanceCutoff` — alterar essas regras impacta `report balance`, `report simulate` e `ConfigRecalculateCommand`.
- **Migração de settings**: `DatabaseEnsurer.Ensure()` faz uma migração silenciosa da tabela `Settings` (legacy) para o JSON. Cuidado ao mexer — só roda uma vez por instalação.
- **Caracteres acentuados** — vários arquivos têm sequências com codificação inconsistente (alguns `Não`/`Ações` foram salvos com mojibake por uma versão antiga do Visual Studio). Há commits recentes corrigindo isso (`ce2e50b`, `041c68d`). Ao editar mensagens em `HhregMessages.cs`, garantir UTF-8 sem BOM e diacríticos preservados.
- **Update self-update**: `UpdateCommand` baixa o release mais novo do GitHub (`vfcardoso-dev/hhreg`), descompacta em `updating/` e dispara um `cmd.exe` que copia por cima e termina. Não tem rollback.
