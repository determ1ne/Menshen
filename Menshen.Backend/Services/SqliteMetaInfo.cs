using System;
using Dapper;
using Menshen.Backend.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Menshen.Backend.Services
{
    public class SqliteMetaInfo : IMetaInfo
    {
        private readonly MemoryCache _memoryCache;
        private readonly SqliteAppDb _db;

        public SqliteMetaInfo(IAppDb db)
        {
            _db = db as SqliteAppDb ?? throw new Exception("No SqliteAppDb instance found.");
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
        }
        
        public string GetValue(string key)
        {
            key = key.ToLowerInvariant().Trim();
            return _memoryCache.GetOrCreate(key, _ =>
            {
                using var conn = _db.GetDbConnection();
                conn.Open();
                var result = conn.ExecuteScalar<string>(@"
SELECT m_value FROM META_INFO WHERE m_key = @Key;
", new { Key = key });
                conn.Close();
                return result;
            });
        }

        public void SetValue(string key, string value)
        {
            key = key.ToLowerInvariant().Trim();
            using var conn = (SqliteConnection) _db.GetDbConnection();
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO META_INFO (m_key, m_value)
  VALUES($m_key, $m_value)
  ON CONFLICT(m_key)
  DO UPDATE SET m_value=excluded.m_value;
";
            cmd.Parameters.AddWithValue("$m_key", key);
            cmd.Parameters.AddWithValue("$m_value", value);
            cmd.ExecuteNonQuery();
            conn.Close();
            _memoryCache.Remove(key);
        }
    }
}

public static class SqliteMetaInfoExtensions
{
    public static IServiceCollection AddSqliteMetaInfo(this IServiceCollection services)
    {
        return services.AddSingleton<IMetaInfo, SqliteMetaInfo>();
    }
}