using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Menshen.Backend.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Menshen.Backend.Services
{
    public class SqliteLocker : ILocker
    {
        private readonly SqliteAppDb _db;

        public SqliteLocker(IAppDb db)
        {
            _db = db as SqliteAppDb ?? throw new Exception("No SqliteAppDb instance found.");
        }
        
        public List<(int, string)> GetSiteContent(string host, int type)
        {
            using var conn = _db.GetDbConnection();
            conn.Open();
            var list = conn.Query<(int, string)>(@"SELECT id, content FROM SITES WHERE host=@host AND type=@type", new {host, type}).ToList();
            return list;
        }

        public void AddSiteConfig(string host, int type, string content)
        {
            using var conn = _db.GetDbConnection();
            conn.Open();
            conn.Execute(@"INSERT INTO SITES (host, type, content) VALUES (@host, @type, @content)", new {host, type, content});
        }

        public (int, long)? GetIpBlockDetails(string ip)
        {
            using var conn = _db.GetDbConnection();
            conn.Open();
            var res = conn.Query<(int, long)?>(@"SELECT count, last_access from BLOCK_DETAILS WHERE ip=@ip", new {ip});
            return res.FirstOrDefault();
        }

        public void BlockIp(string ip, bool permanent)
        {
            using var conn = _db.GetDbConnection();
            conn.Open();
            conn.Execute(@"
INSERT INTO BLOCK_DETAILS (ip, count, last_access) 
  VALUES (@ip, @count, @last_access)
  ON CONFLICT (ip)
  DO UPDATE SET count=count+1", new {ip, count=permanent ? -1 : 1, last_access=DateTimeOffset.UtcNow.ToUnixTimeSeconds()});
        }

        public bool GetIpAllowedForHost(string ip, string host)
        {
            using var conn = _db.GetDbConnection();
            conn.Open();
            var validUntil =
                conn.ExecuteScalar<long?>(@"SELECT valid_until FROM CLIENT_STATUS WHERE ip=@ip AND host=@host",
                    new {ip, host});
            if (validUntil == null || validUntil! < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                return false;
            }

            return true;
        }

        public void AllowIpForHost(string ip, int siteId, string host, TimeSpan timeSpan)
        {
            using var conn = _db.GetDbConnection();
            conn.Open();
            var validUntil = DateTimeOffset.UtcNow.Add(timeSpan).ToUnixTimeSeconds();
            conn.Execute(@"
INSERT INTO CLIENT_STATUS (ip, host, valid_until, site_record_id) 
  VALUES (@ip, @host, @validUntil, @siteId)
  ON CONFLICT (ip, host, site_record_id)
  DO UPDATE SET valid_until = excluded.valid_until", new {ip, host, validUntil, siteId});
            conn.Execute(@"UPDATE BLOCK_DETAILS SET count=0 WHERE ip=@ip", new {ip});
        }
    }
}

public static class SqliteLockerExtensions
{
    public static IServiceCollection AddSqliteLocker(this IServiceCollection services)
    {
        return services.AddSingleton<ILocker, SqliteLocker>();
    }
}