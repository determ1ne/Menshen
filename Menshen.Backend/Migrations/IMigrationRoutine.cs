using System;
using System.Data.Common;

namespace Menshen.Backend.Migrations
{
    public interface IMigrationRoutine
    {
        void Migrate(DbConnection conn, IServiceProvider serviceProvider);
    }
}