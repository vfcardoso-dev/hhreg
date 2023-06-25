using Microsoft.Data.Sqlite;

namespace hhreg.business.infrastructure;

public interface IUnitOfWorkContext
{
    IUnitOfWork Create();
}

public class UnitOfWorkContext : IUnitOfWorkContext {

    private readonly IAppSettings _appSettings;
    private IUnitOfWork? _unitOfWork;

    public UnitOfWorkContext(IAppSettings appSettings) {
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