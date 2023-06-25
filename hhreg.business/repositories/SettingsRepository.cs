using System.Data;
using hhreg.business.domain;
using hhreg.business.infrastructure;

namespace hhreg.business.repositories;

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
        var cmdList = new List<IDbCommand>();
        
        if (newInitialBalance != null) {
            cmdList.Add(_unitOfWork.CreateSqlCommand(
                @"update Settings set InitialBalance = @initialBalance;", 
                    new Dictionary<string, object?> {{"@workDay", newWorkDay}}));
        }

        if (newWorkDay != null) {
            cmdList.Add(_unitOfWork.CreateSqlCommand(
                @"update Settings set WorkDay = @workDay;", 
                    new Dictionary<string, object?> {{"@workDay", newWorkDay}}));
        }

        if (newStartCalculationsAt != null) {
            cmdList.Add(_unitOfWork.CreateSqlCommand(
                @"update Settings set StartCalculationsAt = @startCalculationsAt;", 
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