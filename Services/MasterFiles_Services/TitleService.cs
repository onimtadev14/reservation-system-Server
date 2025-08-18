
using System.Data;
using Microsoft.Data.SqlClient;

public class TitleService
    {
        private readonly string _conn;

        public TitleService(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection");
        }

        public List<Title> GetTitles()
        {
            var list = new List<Title>();

            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_GetAllTitles", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new Title
                {
                    TitleId = Convert.ToInt32(reader["TitleId"]),
                    TitleCode = reader["TitleCode"].ToString(),
                    Description = reader["Description"].ToString()
                });
            }

            return list;
        }
    }



