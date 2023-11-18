namespace Hhreg.Business.Exceptions;

public static class HhregMessages
{
    // Common
    public const string CouldNotParseAsAValidDateFormat = "N�o foi poss�vel converter '{0}' num formato de data v�lido.";
    public const string CouldNotParseAsAValidTimeFormat = "N�o foi poss�vel converter '{0}' num formato de hora v�lido.";
    public const string CouldNotParseAsAValidIntegerFormat = "N�o foi poss�vel converter '{0}' num formato de n�mero inteiro v�lido";
    public const string InvalidInputFormatOnValue = "Formato inv�lido de entrada no valor '{0}'";
    public const string SettingsNotYetInitialized = "CLI ainda n�o foi inicializado.";

    // Config
    public const string DatabaseLocationTitle = "Local do banco de dados";

    // Update
    public const string UnknownPlatform = "UnknownPlatform";
    public const string LastestReleaseUnavailable = "LastestReleaseUnavailable";
    public const string UpdateArtifactNotFound = "UpdateArtifactNotFound";

    // Entry
    public const string YouShouldInformADayToLog = "Voc� deve informar uma data para a entrada (ou definir como hoje com -t).";
    public const string YouShouldInformAtLeastOneTimeEntryOrSetAJustificative = "Voc� deve informar ao menos uma marca��o de hor�rio ou definir uma justificativa com -j.";
    public const string EntryTimesMustBePositive = "Os hor�rios das marca��es devem ser positivos.";
    public const string CannotOverrideANotYetCreatedDay = "N�o � poss�vel sobrescrever um dia que ainda n�o foi criado '{0}'";

    // Init
    public const string YouShouldInformInitialBalance = "Voc� deve informar um saldo inicial.";
    public const string YouShouldInformWorkday = "Voc� deve informar o tempo de um dia de trabalho.";
    public const string YouShouldInformStartCalculationsAt = "Voc� deve informar uma data de corte para o c�lculo do banco de horas.";
    public const string SettingsAlreadyInitialized = "Configura��es j� inicializadas. Voc� pode alter�-las se desejar em 'config edit'";

    // Report
    public const string YouShouldInformADay = "Voc� deve informar um dia.";
    public const string YouShouldInformAMonth = "Voc� deve informar um m�s (MM/yyyy).";
    public const string TailMustHaveAPositiveValue = "Tail deve ser um valor positivo.";
    public const string ThereAreDayEntriesWithAnOddCountOfTimeEntries = "[purple_1]AVISO:[/] Existem entradas com contagem de marca��es �mpar, quando deveriam ser par.";
    public const string PleaseFixTheseDaysBeforeGeneratingNewReports = "Por favor, corrija as entradas antes de gerar novos relat�rios.";
    public const string ConfigurationIsSetToStartBalanceCalculationsAfterTheOffsetDate = "Configura��o define o in�cio dos c�lculos de saldo pra depois do dia solicitado. StartCalculationsAt: {0}; OffsetDate: {1}";
    public const string InformedDayIsNotRegistered = "Dia informado n�o foi lan�ado.";
}