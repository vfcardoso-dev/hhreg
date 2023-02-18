using Dapper;
using Microsoft.Data.Sqlite;

namespace hhreg.business;

public interface IUnitOfWork : IDisposable
{
    void Execute(string query, object? param = null);
    void BulkExecute(string query, IList<object> paramList);
    IEnumerable<T> Query<T>(string query, object? param = null);
    T QuerySingle<T>(string query, object? param = null);
    T QuerySingleOrDefault<T>(string query, object? param = null);
    T QueryFirst<T>(string query, object? param = null);
    T QueryFirstOrDefault<T>(string query, object? param = null);
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

    public void Execute(string query, object? param = null) 
    {
        var conn = GetConnection();
        var tx = conn.BeginTransaction();
        
        try {
            conn.Execute(query, param, tx);
            tx.Commit();
        } catch(Exception) {
            tx.Rollback();
        }
    }

    public void BulkExecute(string query, IList<object> paramList)
    {
        var conn = GetConnection();
        var tx = conn.BeginTransaction();
        
        try {
            foreach(var param in paramList) {
                conn.Execute(query, param, tx);
            }
            tx.Commit();
        } catch(Exception) {
            tx.Rollback();
        }
    }

    public IEnumerable<T> Query<T>(string query, object? param = null)
    {
        return GetConnection().Query<T>(query, param);
    }

    public T QuerySingle<T>(string query, object? param = null)
    {
        return GetConnection().QuerySingle<T>(query, param);
    }

    public T QuerySingleOrDefault<T>(string query, object? param = null)
    {
        return GetConnection().QuerySingleOrDefault<T>(query, param);
    }

    public T QueryFirst<T>(string query, object? param = null)
    {
        return GetConnection().QueryFirst<T>(query, param);
    }

    public T QueryFirstOrDefault<T>(string query, object? param = null)
    {
        return GetConnection().QueryFirstOrDefault<T>(query, param);
    }

    public void Dispose() => _connection?.Close();
}