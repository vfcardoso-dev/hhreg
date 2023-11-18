namespace Hhreg.Business.Exceptions;

public static class HhregMessages
{
    // Common
    public const string CouldNotParseAsAValidDateFormat = "Não foi possível converter '{0}' num formato de data válido.";
    public const string CouldNotParseAsAValidTimeFormat = "Não foi possível converter '{0}' num formato de hora válido.";
    public const string CouldNotParseAsAValidIntegerFormat = "Não foi possível converter '{0}' num formato de número inteiro válido";
    public const string InvalidInputFormatOnValue = "Formato inválido de entrada no valor '{0}'";
    public const string SettingsNotYetInitialized = "CLI ainda não foi inicializado.";

    // Config
    public const string DatabaseLocationTitle = "Local do banco de dados";

    // Update
    public const string UnknownPlatform = "UnknownPlatform";
    public const string LastestReleaseUnavailable = "LastestReleaseUnavailable";
    public const string UpdateArtifactNotFound = "UpdateArtifactNotFound";

    // Entry
    public const string YouShouldInformADayToLog = "Você deve informar uma data para a entrada (ou definir como hoje com -t).";
    public const string YouShouldInformAtLeastOneTimeEntryOrSetAJustificative = "Você deve informar ao menos uma marcação de horário ou definir uma justificativa com -j.";
    public const string EntryTimesMustBePositive = "Os horários das marcações devem ser positivos.";
    public const string CannotOverrideANotYetCreatedDay = "Não é possível sobrescrever um dia que ainda não foi criado '{0}'";

    // Init
    public const string YouShouldInformInitialBalance = "Você deve informar um saldo inicial.";
    public const string YouShouldInformWorkday = "Você deve informar o tempo de um dia de trabalho.";
    public const string YouShouldInformStartCalculationsAt = "Você deve informar uma data de corte para o cálculo do banco de horas.";
    public const string SettingsAlreadyInitialized = "Configurações já inicializadas. Você pode alterá-las se desejar em 'config edit'";

    // Report
    public const string YouShouldInformADay = "Você deve informar um dia.";
    public const string YouShouldInformAMonth = "Você deve informar um mês (MM/yyyy).";
    public const string TailMustHaveAPositiveValue = "Tail deve ser um valor positivo.";
    public const string ThereAreDayEntriesWithAnOddCountOfTimeEntries = "[purple_1]AVISO:[/] Existem entradas com contagem de marcações ímpar, quando deveriam ser par.";
    public const string PleaseFixTheseDaysBeforeGeneratingNewReports = "Por favor, corrija as entradas antes de gerar novos relatórios.";
    public const string ConfigurationIsSetToStartBalanceCalculationsAfterTheOffsetDate = "Configuração define o início dos cálculos de saldo pra depois do dia solicitado. StartCalculationsAt: {0}; OffsetDate: {1}";
    public const string InformedDayIsNotRegistered = "Dia informado não foi lançado.";
}