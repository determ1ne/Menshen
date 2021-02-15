using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Menshen.Backend.Services;
using Menshen.Backend.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Menshen.Backend.Migrations.Routines
{
    [MigrationVersion(1)]
    public class InitDb : IMigrationRoutine
    {
        public void Migrate(DbConnection conn, IServiceProvider serviceProvider)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
CREATE TABLE META_INFO (
m_key TEXT NOT NULL COLLATE NOCASE,
m_value TEXT,
PRIMARY KEY (m_key)
);

CREATE TABLE CLIENT_STATUS (
  id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  ip TEXT NOT NULL COLLATE NOCASE,
  host TEXT NOT NULL,
  valid_until integer NOT NULL,
  site_record_id integer NOT NULL,
  CONSTRAINT site_fk FOREIGN KEY (site_record_id) REFERENCES SITES (id) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT ip_host_siteId_unique UNIQUE (ip COLLATE NOCASE, host COLLATE NOCASE, site_record_id) ON CONFLICT REPLACE 
);

CREATE TABLE BLOCK_DETAILS (
  id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  ip TEXT NOT NULL,
  count integer NOT NULL,
  last_access integer NOT NULL,
  CONSTRAINT ip_unique UNIQUE (ip COLLATE NOCASE) ON CONFLICT REPLACE 
);

CREATE TABLE SITES (
  id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  host TEXT NOT NULL,
  type integer NOT NULL,
  content TEXT NOT NULL
);
";
            cmd.ExecuteNonQuery();

            var configuration = (IConfiguration) serviceProvider.GetService(typeof(IConfiguration));
            var secretHeader = configuration["INIT_REVERSE_PROXY_SECRET_HEADER"];
            var metaInfo = (IMetaInfo) serviceProvider.GetService(typeof(IMetaInfo));
            if (!string.IsNullOrWhiteSpace(secretHeader))
            {
                metaInfo.SetValue(MetaKeys.ReverseProxySecretHeaderName, secretHeader);
            }
            
            // create first admin
            var locker = (ILocker) serviceProvider.GetService(typeof(ILocker));
            var randomChars = new List<char>();
            for (var i = '0'; i <= '9'; ++i) randomChars.Add(i);
            for (var i = 'a'; i <= 'z'; ++i) randomChars.Add(i);
            for (var i = 'A'; i <= 'Z'; ++i) randomChars.Add(i);
            var rcLength = randomChars.Count;
            
            var rng = new Random();
            var password = new StringBuilder();
            for (int i = 0; i < 64; i++) password.Append(randomChars[rng.Next(rcLength)]);
            metaInfo.SetValue(MetaKeys.AdminPassword, BCrypt.Net.BCrypt.EnhancedHashPassword(password.ToString()));

            var logger = (ILogger<InitDb>) serviceProvider.GetService(typeof(ILogger<InitDb>));
            logger.LogInformation($"Your admin account was created. Password: {password}");
            metaInfo.SetValue(MetaKeys.DbVersion, "1");
        }
    }
}