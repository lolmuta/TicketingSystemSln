using System.Data.SqlClient;
using TicketingSystem.Ext;

namespace TicketingSystem.Utils
{
    public class DbHelper
    {
        private readonly IConfiguration _configuration;

        public DbHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public T ConnDb<T>(Func<SqlConnection, T> func)
        {
            var connectionString = _configuration.GetConnectionString("Database");
            using (var sqlconn = new SqlConnection(connectionString))
            {
                sqlconn.Open();
                return func(sqlconn);
            }
        }
        public void ConnDb(Action<SqlConnection> act)
        {
            ConnDb(act.ToFunc());
        }
    }
}
