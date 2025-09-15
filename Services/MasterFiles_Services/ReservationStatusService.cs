using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public interface IReservationStatusService
{
    IEnumerable<ReservationStatus> GetVisibleStatuses();
}

public class ReservationStatusService : IReservationStatusService
{
    private readonly string _conn;

    public ReservationStatusService(IConfiguration config)
    {
        _conn = config.GetConnectionString("DefaultConnection");
    }

    public IEnumerable<ReservationStatus> GetVisibleStatuses()
    {
        var statuses = new List<ReservationStatus>();

        using (SqlConnection conn = new SqlConnection(_conn))
        using (SqlCommand cmd = new SqlCommand("sp_GetAllReservationStatus", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            conn.Open();

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    statuses.Add(new ReservationStatus
                    {
                        StatusId = reader.GetInt32(reader.GetOrdinal("statusid")),
                        StatusName = reader["statusname"].ToString(),
                        RowNo = reader["rowno"] as int?,
                        StatusType = reader["statustype"].ToString(),
                        ColorCode = reader["colorcode"].ToString(),
                        IsShow = reader.GetBoolean(reader.GetOrdinal("isShow"))
                    });
                }
            }
        }

        return statuses;
    }
}
