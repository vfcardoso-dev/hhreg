using Microsoft.Data.Sqlite;

namespace Hhreg.Business.Infrastructure;

public interface IUnitOfWorkContext
{
    IUnitOfWork Create();
}

public class UnitOfWorkContext : IUnitOfWorkContext
{

    private readonly ISettingsService _appSettings;
    private IUnitOfWork? _unitOfWork;

    public UnitOfWorkContext(ISettingsService appSettings)
    {
        _appSettings = appSettings;
    }

    public IUnitOfWork Create()
    {
        if (_unitOfWork == null)
        {
            var connection = new SqliteConnection(_appSettings.ConnectionString);
            _unitOfWork = new UnitOfWork(connection);
        }

        return _unitOfWork;
    }
}