using System.Data;
using Microsoft.Data.SqlClient;

namespace YourNamespace.Services
{
    public class BookingResourceService
    {
        private readonly IConfiguration _config;

        public BookingResourceService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<List<BookingResource>> GetAllBookingResourcesAsync()
        {
            var list = new List<BookingResource>();

            using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("SELECT ReservationBookingResourceId, BookingResourceName FROM SV_Reservation.dbo.Reservation_BookingResource", conn);

            try
            {
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new BookingResource
                    {
                        ReservationBookingResourceId = reader.GetInt32(0),
                        BookingResourceName = reader.GetString(1)
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching booking resources: {ex.Message}");
                throw;
            }

            return list;
        }
    }
}
