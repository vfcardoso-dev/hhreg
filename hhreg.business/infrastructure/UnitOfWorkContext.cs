using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace hhreg.business;

public interface IUnitOfWorkContext
{
    IUnitOfWork Create();
}

public class UnitOfWorkContext : IUnitOfWorkContext {

    private readonly AppSettings _appSettings;
    private readonly ILogger<UnitOfWork> _logger;
    private IUnitOfWork? _unitOfWork;

    public UnitOfWorkContext(AppSettings appSettings, ILoggerFactory loggerFactory) {
        _logger = loggerFactory.CreateLogger<UnitOfWork>();
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