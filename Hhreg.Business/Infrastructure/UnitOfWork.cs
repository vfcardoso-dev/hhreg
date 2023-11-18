using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Hhreg.Business.Infrastructure;

public interface IUnitOfWork : IDisposable
{
    void Execute(string query, object? param = null);
    void BulkExecute(IEnumerable<IDbCommand> commands);
    IDbCommand CreateSqlCommand(string query, IDictionary<string, object?>? param = null);
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

    private SqliteConnection GetConnection()
    {
        _connection.Open();
        return _connection;
    }

    public void Execute(string query, object? param = null)
    {
        var conn = GetConnection();
        var tx = conn.BeginTransaction();

        try
        {
            conn.Execute(query, param, tx);
            tx.Commit();
        }
        catch (Exception)
        {
            tx.Rollback();
            throw;
        }
    }

    public void BulkExecute(IEnumerable<IDbCommand> commands)
    {
        var conn = GetConnection();
        var tx = conn.BeginTransaction();

        try
        {
            foreach (var cmd in commands)
            {
                cmd.Connection = conn;
                cmd.Transaction = tx;
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }
            tx.Commit();
        }
        catch (Exception)
        {
            tx.Rollback();
            throw;
        }
    }

    public IDbCommand CreateSqlCommand(string query, IDictionary<string, object?>? param = null)
    {
        var cmd = new SqliteCommand(query);

        if (param != null)
        {
            foreach (var paramEntry in param)
            {
                cmd.Parameters.AddWithValue(paramEntry.Key, paramEntry.Value ?? DBNull.Value);
            }
        }

        return cmd;
    }

    public IEnumerable<T> Query<T>(string query, object? param = null)
    {
        return GetConnection().Query<T>(query, param);
    }

    public IEnumerable<T3> Query<T1, T2, T3>(string query, Func<T1, T2, T3> action, object? param = null, string? splitOn = null)
    {
        return GetConnection().Query(query, action, param, splitOn: splitOn);
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

    public void Dispose()
    {
        _connection?.Close();
        GC.SuppressFinalize(this);
    }
}