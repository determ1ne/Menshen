using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace Menshen.Backend.Services
{
    public interface IAppDb
    {
        DbConnection GetDbConnection();
    }
}