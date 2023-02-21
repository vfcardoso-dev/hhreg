using hhreg.business;
using hhreg.business.domain;
using Microsoft.Data.Sqlite;

public interface ISettingsRepository {
    void Create(double initialBalance, double workDay, string startCalculationsAt);
    void Update(double? newInitialBalance, double? newWorkDay, string? newStartCalculationsAt);
    Settings GetOrDefault();
    Settings Get();
    bool IsAlreadyInitialized();
}

public class SettingsRepository : ISettingsRepository
{
    private readonly IUnitOfWork _unitOfWork;

    public SettingsRepository(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public void Create(double initialBalance, double workDay, string startCalculationsAt)
    {
        _unitOfWork.Execute(
                @"insert into Settings (InitialBalance, WorkDay, StartCalculationsAt) 
                    values (@initialBalance, @workDay, @startCalculationsAt);", new { initialBalance, workDay, startCalculationsAt });
    }

    public void Update(double? newInitialBalance, double? newWorkDay, string? newStartCalculationsAt)
    {
        var cmdList = new List<SqliteCommand>();
        
        if (newInitialBalance != null) {
            cmdList.Add(_unitOfWork.CreateSqlCommand(
                @"update Settings set InitialBalance = @initialBalance limit 1;", 
                    new Dictionary<string, object?> {{"@workDay", newWorkDay}}));
        }

        if (newWorkDay != null) {
            cmdList.Add(_unitOfWork.CreateSqlCommand(
                @"update Settings set WorkDay = @workDay limit 1;", 
                    new Dictionary<string, object?> {{"@workDay", newWorkDay}}));
        }

        if (newStartCalculationsAt != null) {
            cmdList.Add(_unitOfWork.CreateSqlCommand(
                @"update Settings set StartCalculationsAt = @startCalculationsAt limit 1;", 
                    new Dictionary<string, object?> {{"@startCalculationsAt", newStartCalculationsAt}}));
        }

        _unitOfWork.BulkExecute(cmdList);
    }

    public Settings Get() => 
        _unitOfWork.QuerySingle<Settings>("select * from Settings limit 1");

    public Settings GetOrDefault() => 
        _unitOfWork.QuerySingleOrDefault<Settings>("select * from Settings limit 1");

    public bool IsAlreadyInitialized() => 
        _unitOfWork.QuerySingleOrDefault<int>("select count(*) from Settings") > 0;
}