using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;

public class NationalityService
{
    private readonly string _conn;

    public NationalityService(IConfiguration config)
    {
        _conn = config.GetConnectionString("DefaultConnection");
    }

    public List<Nationality> GetAllNationalities()
    {
        var list = new List<Nationality>();

        using var conn = new SqlConnection(_conn);
        using var cmd = new SqlCommand("sp_GetAllNationalities", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Nationality
            {
                NationalityID = Convert.ToInt32(reader["NationalityID"]),
                NationalityCode = reader["NationalityCode"].ToString(),
                Description = reader["Description"].ToString()
            });
        }

        return list;
    }
}
