using System.Data.Common;
using Menshen.Backend.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace Menshen.Backend.Services
{
    public class SqliteAppDb : IAppDb
    {
        private readonly string connectionString;

        public SqliteAppDb(string dataSource)
        {
            // run vacuum before wal mode
            // so page size can be adjusted
            var csBuilder = new SqliteConnectionStringBuilder();
            csBuilder.DataSource = dataSource;
            csBuilder.Mode = SqliteOpenMode.ReadWriteCreate;
            csBuilder.Cache = SqliteCacheMode.Shared;
            connectionString = csBuilder.ConnectionString;
        }

        public DbConnection GetDbConnection()
        {
            return new SqliteConnection(connectionString);
        }
    }
}

public static class SqliteAppDbExtensions
{
    public static IServiceCollection AddSqliteAppDb(this IServiceCollection services, string dataSource)
    {
        return services.AddTransient<IAppDb>(_ => new SqliteAppDb(dataSource));
    }
}