using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

public interface IReservationCalendarService
{
    Task<List<ReservationCalendarDto>> GetReservationCalendarDataAsync(DateTime start, DateTime end, int calendarType);
}

public class ReservationCalendarService : IReservationCalendarService
{
    private readonly string _connectionString;

    public ReservationCalendarService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<List<ReservationCalendarDto>> GetReservationCalendarDataAsync(DateTime start, DateTime end, int calendarType)
    {
        using var connection = new SqlConnection(_connectionString);

        var result = await connection.QueryAsync<dynamic>(
            "sp_GetReservationCalendarData",
            new { StartDateTime = start, EndDateTime = end, CalendarType = calendarType },
            commandType: CommandType.StoredProcedure);

        var list = new List<ReservationCalendarDto>();

        foreach (var row in result)
        {
            var dict = new Dictionary<string, string>();
            var obj = (IDictionary<string, object>)row;

            var dto = new ReservationCalendarDto
            {
                RoomId = Convert.ToInt32(obj["RoomId"]),
                RoomName = obj["RoomName"]?.ToString(),
                RoomSize = obj["RoomSize"]?.ToString()
            };

            foreach (var col in obj.Keys)
            {
                if (col != "RoomId" && col != "RoomName" && col != "RoomSize")
                {
                    dict[col] = obj[col]?.ToString();
                }
            }

            dto.CalendarDetails = dict;
            list.Add(dto);
        }

        return list;
    }
}
