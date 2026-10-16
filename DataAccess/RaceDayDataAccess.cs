
using Microsoft.Data.SqlClient;
using RaceDay222.DatabaseContext;

namespace RaceDay222.DataAccess
{
    public class RaceDayDataAccess
    {
        private readonly RaceDayContext _context;

        public RaceDayDataAccess(RaceDayContext context)
        {
            _context = context;
        }

        public SqlConnection CreateConnection()
        {
            return _context.GetConnection();
        }
    }
}