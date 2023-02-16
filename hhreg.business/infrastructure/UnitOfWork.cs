using Dapper;
using Microsoft.Data.Sqlite;

namespace hhreg.business;

public interface IUnitOfWork : IDisposable
{
    void Execute(string query, object? param = null);
    IEnumerable<T> Query<T>(string query);
    T QuerySingle<T>(string query);
    T QuerySingleOrDefault<T>(string query);
    T QueryFirst<T>(string query);
    T QueryFirstOrDefault<T>(string query);
}

public class UnitOfWork : IUnitOfWork
{
    private readonly SqliteConnection _connection;

    public UnitOfWork(SqliteConnection connection)
    {
        _connection = connection;
    }

    private SqliteConnection GetConnection() {
        _connection.Open();
        return _connection;
    }

    public void Execute(string query, object? param = null) {
        var conn = GetConnection();
        var tx = conn.BeginTransaction();
        
        try {
            conn.Execute(query, param, tx);
            tx.Commit();
        } catch(Exception) {
            tx.Rollback();
        }
    }

    public IEnumerable<T> Query<T>(string query) => GetConnection().Query<T>(query);

    public T QuerySingle<T>(string query) => GetConnection().QuerySingle<T>(query);

    public T QuerySingleOrDefault<T>(string query) => GetConnection().QuerySingleOrDefault<T>(query);

    public T QueryFirst<T>(string query) => GetConnection().QueryFirst<T>(query);

    public T QueryFirstOrDefault<T>(string query) => GetConnection().QueryFirstOrDefault<T>(query);

    public void Dispose() => _connection?.Close();
}