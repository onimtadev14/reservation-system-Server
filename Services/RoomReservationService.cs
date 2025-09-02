using Microsoft.Data.SqlClient;
using System.Data;

public interface IRoomReservationService
{
    Task<string> SaveOrUpdateReservationAsync(ReservationDto dto);
    Task<List<ReservationDto>> GetAllReservationsAsync();

}





public class RoomReservationService : IRoomReservationService
{
    private readonly IConfiguration _config;
    public RoomReservationService(IConfiguration config) => _config = config;

    public async Task<List<ReservationDto>> GetAllReservationsAsync()
{
    var list = new List<ReservationDto>();

    using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
    using var cmd  = new SqlCommand("sp_reservation_get_all", conn) { CommandType = CommandType.StoredProcedure };

    await conn.OpenAsync();
    using var rdr = await cmd.ExecuteReaderAsync();

    while (await rdr.ReadAsync())
    {
        list.Add(new ReservationDto
        {
            ReservationNo     = rdr["ReservationNo"]?.ToString(),
            ReservationDate   = rdr.GetDateTime(rdr.GetOrdinal("ReservationDate")),
            ReservationType   = rdr.GetInt32(rdr.GetOrdinal("ReservationType")),
            CustomerCode      = rdr["CustomerCode"]?.ToString(),
            Mobile            = rdr["Mobile"]?.ToString(),
            Telephone         = rdr["Telephone"]?.ToString(),
            Email             = rdr["Email"]?.ToString(),
            TravelAgentCode   = rdr["TravelAgentCode"]?.ToString(),
            CheckinDateTime   = rdr.GetDateTime(rdr.GetOrdinal("Checkindatetime")),
            CheckoutDateTime  = rdr.GetDateTime(rdr.GetOrdinal("Checkoutdatetime")),
            NoOfVehicles      = rdr.GetInt32(rdr.GetOrdinal("NoOfVehicles")),
            NoOfAdults        = rdr.GetInt32(rdr.GetOrdinal("NoOfAdults")),
            NoOfKids          = rdr.GetInt32(rdr.GetOrdinal("NoOfKids")),
            EventType         = rdr["EventType"]?.ToString(),
            SetupStyle        = rdr["SetupStyle"]?.ToString(),
            SubTotal          = rdr.GetDecimal(rdr.GetOrdinal("SubTotal")),
            DiscountPer       = rdr.GetDecimal(rdr.GetOrdinal("DiscountPer")),
            Discount          = rdr.GetDecimal(rdr.GetOrdinal("Discount")),
            GrossAmount       = rdr.GetDecimal(rdr.GetOrdinal("GrossAmount")),
            PaidAmount        = rdr.GetDecimal(rdr.GetOrdinal("PaidAmount")),
            DueAmount         = rdr.GetDecimal(rdr.GetOrdinal("DueAmount")),
            ReservationNote   = rdr["ReservationNote"]?.ToString(),
            RefundAmount      = rdr.GetDecimal(rdr.GetOrdinal("RefundAmount")),
            RefundNote        = rdr["RefundNote"]?.ToString(),
            ReferenceNo       = rdr["ReferenceNo"]?.ToString(),
            BookingResourceId = rdr.GetInt32(rdr.GetOrdinal("BookingResourceId")),
            BookingReferenceNo= rdr["BookingReference"]?.ToString(),
            ReservationStatus = rdr["ReservationStatus"]?.ToString(),
            User              = rdr["User"]?.ToString()
        });
    }

    return list;
}


    public async Task<string> SaveOrUpdateReservationAsync(ReservationDto dto)
    {
        using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        using var cmd = new SqlCommand("sp_reservation_save", conn) { CommandType = CommandType.StoredProcedure };

        try
        {
            // ---- Scalar params (typed; sizes match SP) ----
            AddVarchar(cmd, "@ReservationNo", 300, (dto.ReservationNo ?? "").Trim()); // send "" for new; SP checks ISNULL(...,'')=''
            AddDateTime(cmd, "@ReservationDate", dto.ReservationDate);
            AddInt(cmd, "@ReservationType", dto.ReservationType);
            AddVarchar(cmd, "@CustomerCode", 300, dto.CustomerCode);

            AddVarcharNullable(cmd, "@Mobile", 300, dto.Mobile);
            AddVarcharNullable(cmd, "@Telephone", 300, dto.Telephone);
            AddVarcharNullable(cmd, "@email", 300, dto.Email);
            AddVarcharNullable(cmd, "@TravelAgentCode", 300, dto.TravelAgentCode);

            AddDateTime(cmd, "@Checkindatetime", dto.CheckinDateTime);
            AddDateTime(cmd, "@checkoutdatetime", dto.CheckoutDateTime);

            AddInt(cmd, "@noofVehicles", dto.NoOfVehicles);
            AddInt(cmd, "@noofadults", dto.NoOfAdults);
            AddInt(cmd, "@noofKids", dto.NoOfKids);

            AddVarcharNullable(cmd, "@EventType", 100, dto.EventType);
            AddVarcharNullable(cmd, "@SetupStyle", 100, dto.SetupStyle);

            AddMoney(cmd, "@SubTotal", dto.SubTotal);
            AddMoney(cmd, "@DiscountPer", dto.DiscountPer);
            AddMoney(cmd, "@Discount", dto.Discount);
            AddMoney(cmd, "@GrossAmount", dto.GrossAmount);
            AddMoney(cmd, "@PaidAmount", dto.PaidAmount);
            AddMoney(cmd, "@DueAmount", dto.DueAmount);

            AddVarcharNullable(cmd, "@ReservationNote", 300, dto.ReservationNote);
            AddMoney(cmd, "@RefundAmount", dto.RefundAmount);
            AddVarcharNullable(cmd, "@RefundNote", 100, dto.RefundNote);
            AddVarcharNullable(cmd, "@ReferenceNo", 50, dto.ReferenceNo);

            AddInt(cmd, "@BookingResourceId", dto.BookingResourceId);
            AddVarcharNullable(cmd, "@BookingReferenceNo", 50, dto.BookingReferenceNo);
            AddVarcharNullable(cmd, "@ReservationStatus", 100, dto.ReservationStatus);
            AddVarcharNullable(cmd, "@User", 50, dto.User);

            // ---- TVPs (schemas EXACTLY how SP uses them) ----
            var dtRoom = CreateRoomDetailsTable(dto.RoomDetails);
            var pRoom = cmd.Parameters.Add("@dt_RoomDetails", SqlDbType.Structured);
            pRoom.TypeName = "dt_RoomDetails";
            pRoom.Value = dtRoom;

            var dtSvc = CreateServiceDetailsTable(dto.ServiceDetails);
            var pSvc = cmd.Parameters.Add("@dt_ServiceDetails", SqlDbType.Structured);
            pSvc.TypeName = "dt_ServiceDetails";
            pSvc.Value = dtSvc;

            var dtPay = CreateRoomPayTable(dto.RoomPayDetails);
            var pPay = cmd.Parameters.Add("@dt_RoomPayDetails", SqlDbType.Structured);
            pPay.TypeName = "dt_RoomPayDetails";
            pPay.Value = dtPay;

            // ---- Output ----
            var outResNo = new SqlParameter("@ReservationNoRet", SqlDbType.VarChar, 20) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(outResNo);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            return outResNo.Value?.ToString() ?? "";
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"SQL Error: {ex.Message}");
            throw new Exception("Database error occurred while saving reservation.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw new Exception("An error occurred while saving reservation.");
        }
    }

    // ---------- TVP builders that match the SP ----------

    // @dt_RoomDetails used by MERGE:
    // INSERT (..., [checkindate], [checkoutdate]) VALUES (..., MySource.[checkin], MySource.[checkout])
    // dt_RoomDetails: exactly the columns the SP uses in MERGE (order matters)
    private static DataTable CreateRoomDetailsTable(List<RoomDetailDto>? list)
    {
        var t = new DataTable();
        t.Columns.Add("RoomCode", typeof(string));   // 1
        t.Columns.Add("PackageCode", typeof(string));   // 2
        t.Columns.Add("noofdays", typeof(int));      // 3
        t.Columns.Add("Price", typeof(decimal));  // 4  DECIMAL(18,2) compatible
        t.Columns.Add("Amount", typeof(decimal));  // 5  DECIMAL(18,2) compatible
        t.Columns.Add("checkin", typeof(DateTime)); // 6
        t.Columns.Add("checkout", typeof(DateTime)); // 7

        foreach (var x in list ?? Enumerable.Empty<RoomDetailDto>())
        {
            t.Rows.Add(
                x.RoomCode ?? (object)DBNull.Value,
                x.PackageCode ?? (object)DBNull.Value,
                x.NoOfDays,
                x.Price,
                x.Amount,
                x.CheckinDate,
                x.CheckoutDate
            );
        }
        return t;
    }

    // dt_ServiceDetails: SP uses these names (no extra "Service" column)
    private static DataTable CreateServiceDetailsTable(List<ServiceDetailDto>? list)
    {
        var t = new DataTable();
        t.Columns.Add("ServiceTypeCode", typeof(string));   // 1
        t.Columns.Add("ServiceQuantity", typeof(int));      // 2
        t.Columns.Add("ServiceAmount", typeof(decimal));  // 3
        t.Columns.Add("ServiceTotalAmount", typeof(decimal));  // 4
        t.Columns.Add("ServiceDate", typeof(DateTime)); // 5
        t.Columns.Add("ServiceRemark", typeof(string));   // 6

        foreach (var x in list ?? Enumerable.Empty<ServiceDetailDto>())
        {
            t.Rows.Add(
                x.ServiceTypeCode ?? (object)DBNull.Value,
                x.ServiceQuantity,
                x.ServiceAmount,
                x.ServiceTotalAmount,
                x.ServiceDate,
                x.ServiceRemark ?? (object)DBNull.Value
            );
        }
        return t;
    }

    // dt_RoomPayDetails: SP reads these (no PaymentType column)
    private static DataTable CreateRoomPayTable(List<RoomPaymentDetailDto>? list)
    {
        var t = new DataTable();
        t.Columns.Add("ReceiptNo", typeof(string));    // 1
        t.Columns.Add("PaymentID", typeof(int));       // 2  use long if BIGINT in SQL
        t.Columns.Add("Amount", typeof(decimal));   // 3
        t.Columns.Add("RefNo", typeof(string));    // 4
        t.Columns.Add("RefDate", typeof(DateTime));  // 5

        foreach (var x in list ?? Enumerable.Empty<RoomPaymentDetailDto>())
        {
            t.Rows.Add(
                x.ReceiptNo ?? (object)DBNull.Value,
                x.PaymentId,
                x.Amount,
                x.RefNo ?? (object)DBNull.Value,
                x.RefDate ?? (object)DBNull.Value
            );
        }
        return t;
    }


    // ---------- Parameter helpers (typed; sizes & scales set) ----------

    private static void AddVarchar(SqlCommand cmd, string name, int size, string value)
    {
        var p = cmd.Parameters.Add(name, SqlDbType.VarChar, size);
        p.Value = value;
    }

    private static void AddVarcharNullable(SqlCommand cmd, string name, int size, string? value)
    {
        var p = cmd.Parameters.Add(name, SqlDbType.VarChar, size);
        p.Value = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value!.Trim();
    }

    private static void AddInt(SqlCommand cmd, string name, int value)
    {
        var p = cmd.Parameters.Add(name, SqlDbType.Int);
        p.Value = value;
    }

    private static void AddDateTime(SqlCommand cmd, string name, DateTime value)
    {
        var p = cmd.Parameters.Add(name, SqlDbType.DateTime);
        p.Value = value;
    }

    private static void AddMoney(SqlCommand cmd, string name, decimal value)
    {
        var p = cmd.Parameters.Add(name, SqlDbType.Decimal);
        p.Precision = 18;
        p.Scale = 2;
        p.Value = value;
    }
    


}
