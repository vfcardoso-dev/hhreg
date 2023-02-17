using System.Diagnostics.CodeAnalysis;
using hhreg.business;
using hhreg.business.domain;

public interface ISettingsRepository {
    void Create(double initialBalance, double workDay);
    void Update(double newInitialBalance, double newWorkDay);
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

    public void Create(double initialBalance, double workDay)
    {
        _unitOfWork.Execute(
                @"insert into Settings (InitialBalance, WorkDay, LunchTime) 
                    values (@initialBalance, @workDay, 60);", new { initialBalance, workDay });
    }

    public void Update(double newInitialBalance, double newWorkDay)
    {
        _unitOfWork.Execute(
                @"update Settings set InitialBalance = @newInitialBalance, WorkDay = @newWorkDay limit 1;", 
                    new { newInitialBalance, newWorkDay });
    }

    public Settings Get() => 
        _unitOfWork.QuerySingle<Settings>("select * from Settings limit 1");

    public Settings GetOrDefault() => 
        _unitOfWork.QuerySingleOrDefault<Settings>("select * from Settings limit 1");

    public bool IsAlreadyInitialized() => 
        _unitOfWork.QuerySingleOrDefault<int>("select count(*) from Settings") > 0;
}