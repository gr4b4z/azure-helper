using System.Data.SqlClient;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class MsSqlExecutor
    {
        private readonly string _connectionString;

        public MsSqlExecutor(string connectionString)
        {
            _connectionString = connectionString;
        }
        public Task ExecuteSqlAsync(string data)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                using (var command = new SqlCommand(data, con))
                {
                     command.ExecuteNonQuery();
                    return Task.CompletedTask;
                }
            }
        }
    }
}
