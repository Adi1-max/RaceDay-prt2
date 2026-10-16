
using Microsoft.Data.SqlClient;

namespace RaceDay222.DatabaseContext
{
    public class RaceDayContext 
    {
        internal readonly object Database;
        private readonly IConfiguration _configuration;

        public RaceDayContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public object Context { get; internal set; }

        public SqlConnection GetConnection()
        {
            string connectionString =
                _configuration.GetConnectionString("RaceDayConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'RaceDayConnection' was not found.");

            return new SqlConnection(connectionString);
        }
    }
}