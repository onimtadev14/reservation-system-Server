using Microsoft.Data.SqlClient;
using System.Data;

public interface IRoomReservationService
{
    Task<string> SaveOrUpdateReservationAsync(ReservationDto dto);
}

public class RoomReservationService : IRoomReservationService
{
    private readonly IConfiguration _config;

    public RoomReservationService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<string> SaveOrUpdateReservationAsync(ReservationDto dto)
    {
        using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        using var cmd = new SqlCommand("sp_reservation_save", conn);
        cmd.CommandType = CommandType.StoredProcedure;

        try
        {

            Console.WriteLine(dto.ReservationNo);
            // 🔹 Normal parameters
            cmd.Parameters.AddWithValue("@ReservationNo", (object?)dto.ReservationNo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ReservationDate", dto.ReservationDate);
            cmd.Parameters.AddWithValue("@ReservationType", dto.ReservationType);
            cmd.Parameters.AddWithValue("@CustomerCode", dto.CustomerCode);
            cmd.Parameters.AddWithValue("@Mobile", dto.Mobile ?? "");
            cmd.Parameters.AddWithValue("@Telephone", dto.Telephone ?? "");
            cmd.Parameters.AddWithValue("@Email", dto.Email ?? "");
            cmd.Parameters.AddWithValue("@TravelAgentCode", dto.TravelAgentCode ?? "");
            cmd.Parameters.AddWithValue("@Checkindatetime", dto.CheckinDateTime);
            cmd.Parameters.AddWithValue("@checkoutdatetime", dto.CheckoutDateTime);
            cmd.Parameters.AddWithValue("@noofVehicles", dto.NoOfVehicles);
            cmd.Parameters.AddWithValue("@noofadults", dto.NoOfAdults);
            cmd.Parameters.AddWithValue("@noofKids", dto.NoOfKids);
            cmd.Parameters.AddWithValue("@EventType", dto.EventType ?? "");
            cmd.Parameters.AddWithValue("@SetupStyle", dto.SetupStyle ?? "");
            cmd.Parameters.AddWithValue("@SubTotal", dto.SubTotal);
            cmd.Parameters.AddWithValue("@DiscountPer", dto.DiscountPer);
            cmd.Parameters.AddWithValue("@Discount", dto.Discount);
            cmd.Parameters.AddWithValue("@GrossAmount", dto.GrossAmount);
            cmd.Parameters.AddWithValue("@PaidAmount", dto.PaidAmount);
            cmd.Parameters.AddWithValue("@DueAmount", dto.DueAmount);
            cmd.Parameters.AddWithValue("@ReservationNote", dto.ReservationNote ?? "");
            cmd.Parameters.AddWithValue("@RefundAmount", dto.RefundAmount);
            cmd.Parameters.AddWithValue("@RefundNote", dto.RefundNote ?? "");
            cmd.Parameters.AddWithValue("@ReferenceNo", dto.ReferenceNo ?? "");
            cmd.Parameters.AddWithValue("@BookingResourceId", dto.BookingResourceId);
            cmd.Parameters.AddWithValue("@BookingReferenceNo", dto.BookingReferenceNo ?? "");
            cmd.Parameters.AddWithValue("@ReservationStatus", dto.ReservationStatus ?? "");
            cmd.Parameters.AddWithValue("@User", dto.User ?? "");

            // 🔹 TVPs
            cmd.Parameters.Add(new SqlParameter("@dt_RoomDetails", SqlDbType.Structured)
            {
                TypeName = "dt_RoomDetails",
                Value = CreateRoomDetailsTable(dto.RoomDetails)
            });
            cmd.Parameters.Add(new SqlParameter("@dt_ServiceDetails", SqlDbType.Structured)
            {
                TypeName = "dt_ServiceDetails",
                Value = CreateServiceDetailsTable(dto.ServiceDetails)
            });
            cmd.Parameters.Add(new SqlParameter("@dt_RoomPayDetails", SqlDbType.Structured)
            {
                TypeName = "dt_RoomPayDetails",
                Value = CreateRoomPayTable(dto.RoomPayDetails)
            });

            // 🔹 Output parameter
            var outputParam = new SqlParameter("@ReservationNoRet", SqlDbType.VarChar, 20)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(outputParam);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            return outputParam.Value?.ToString() ?? "";
        }
        catch (SqlException sqlEx)
        {
            // Log SQL-specific errors
            Console.WriteLine($"SQL Error: {sqlEx.Message}");
            throw new Exception("Database error occurred while saving reservation.");
        }
        catch (Exception ex)
        {
            // Log general errors
            Console.WriteLine($"Error: {ex.Message}");
            throw new Exception("An error occurred while saving reservation.");
        }
    }

    // ✅ Convert Lists to DataTable for TVPs (methods remain the same)
    private DataTable CreateRoomDetailsTable(List<RoomDetailDto> list)
    {
        var table = new DataTable();
        table.Columns.Add("RoomCode", typeof(string));
        table.Columns.Add("PackageCode", typeof(string));
        table.Columns.Add("NoOfDays", typeof(int));
        table.Columns.Add("Price", typeof(decimal));
        table.Columns.Add("Amount", typeof(decimal));
        table.Columns.Add("Checkin", typeof(DateTime));
        table.Columns.Add("Checkout", typeof(DateTime));

        if (list != null)
        {
            foreach (var item in list)
            {
                table.Rows.Add(
                    item.RoomCode, 
                    item.PackageCode, 
                    item.NoOfDays, 
                    item.Price, 
                    item.Amount, 
                    item.Checkin, 
                    item.Checkout
                );
            }
        }
        return table;
    }

    private DataTable CreateServiceDetailsTable(List<ServiceDetailDto> list)
    {
        var table = new DataTable();
        table.Columns.Add("ServiceTypeCode", typeof(string));
        table.Columns.Add("ServiceQuantity", typeof(int));
        table.Columns.Add("ServiceAmount", typeof(decimal));
        table.Columns.Add("ServiceTotalAmount", typeof(decimal));
        table.Columns.Add("ServiceDate", typeof(DateTime));
        table.Columns.Add("ServiceRemark", typeof(string));

        if (list != null)
        {
            foreach (var item in list)
            {
                table.Rows.Add(
                    item.ServiceTypeCode, 
                    item.ServiceQuantity, 
                    item.ServiceAmount, 
                    item.ServiceTotalAmount, 
                    item.ServiceDate, 
                    item.ServiceRemark
                );
            }
        }
        return table;
    }

    private DataTable CreateRoomPayTable(List<RoomPaymentDetailDto> list)
    {
        var table = new DataTable();
        table.Columns.Add("PaymentId", typeof(int));
        table.Columns.Add("Amount", typeof(decimal));
        table.Columns.Add("RefNo", typeof(string));
        table.Columns.Add("RefDate", typeof(DateTime));
        table.Columns.Add("ReceiptNo", typeof(string));

        if (list != null)
        {
            foreach (var item in list)
            {
                table.Rows.Add(
                    item.PaymentId, 
                    item.Amount, 
                    item.RefNo, 
                    item.RefDate ?? (object)DBNull.Value, 
                    item.ReceiptNo ?? ""
                );
            }
        }
        return table;
    }
}
