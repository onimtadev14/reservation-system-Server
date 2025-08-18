using System.Data;
using Microsoft.Data.SqlClient;

namespace OIT_Reservation.Services
{
    public class CountryService
    {
        private readonly string _conn;

        public CountryService(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection");
        }

        public List<Country> GetAllCountries()
        {
            var list = new List<Country>();

            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_GetAllCountries", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new Country
                {
                    CountryID = Convert.ToInt32(reader["CountryID"]),
                    CountryCode = reader["CountryCode"].ToString(),
                    Description = reader["Description"].ToString()
                });
            }

            return list;
        }


    }
}