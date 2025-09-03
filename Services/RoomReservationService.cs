using Microsoft.Data.SqlClient;
using System.Data;

public interface IRoomReservationService
{
    Task<string> SaveOrUpdateReservationAsync(ReservationDto dto);
    Task<IReadOnlyList<ReservationDto>> GetAllAsync(int? top = null);

}

public class RoomReservationService : IRoomReservationService
{
    private readonly IConfiguration _config;

    private static object ToDbDate(DateTime value) =>
    value == DateTime.MinValue ? DBNull.Value : value;

    private static object ToDbDate(DateTime? value) =>
        value == null || value == DateTime.MinValue ? DBNull.Value : value;

    public RoomReservationService(IConfiguration config) => _config = config;

    public async Task<IReadOnlyList<ReservationDto>> GetAllAsync(int? top = null)
    {
        using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        // Build TOP (@top) only when requested
        var topSql = top.HasValue ? "TOP (@top) " : string.Empty;

        var sql = $@"
                    SELECT {topSql}
                        h.ReservationNo,
                        h.ReservationDate,
                        h.ReservationType,
                        h.CustomerCode,
                        h.Mobile,
                        h.Telephone,
                        h.email,
                        h.TravelAgentCode,
                        h.Checkindatetime,
                        h.checkoutdatetime,
                        h.noofVehicles,
                        h.noofadults,
                        h.noofKids,
                        h.eventtype,
                        h.setupstyle,
                        h.SubTotal,
                        h.DiscountPer,
                        h.Discount,
                        h.GrossAmount,
                        h.PaidAmount,
                        h.DueAmount,
                        h.Remark,
                        h.RefundAmount,
                        h.refundnote,
                        h.ReferenceReservationNo,
                        h.BookingResourceId,
                        h.BookingReference,
                        h.ReservationStatus,
                        h.crUser,

                        -- Room details (LEFT JOIN; may be null)
                        rd.ReservationRoomDetailsID,
                        rd.RoomCode,
                        rd.PackageCode,
                        rd.noofdays,
                        rd.Price    AS rdPrice,
                        rd.Amount   AS rdAmount,
                        rd.IsDelete,
                        rd.ModifiedDate,
                        rd.checkindate,
                        rd.checkoutdate,

                        -- Service details
                        sd.ServiceCode       AS ServiceTypeCode,
                        sd.ServiceDate,
                        sd.ServiceQty        AS ServiceQuantity,
                        sd.Amount            AS ServiceAmount,
                        sd.TotalAmount       AS ServiceTotalAmount,
                        sd.serviceremark,

                        -- Payment details
                        pd.PaymentID         AS PaymentId,
                        pd.Amount            AS PayAmount,
                        pd.RefNo,
                        pd.RefDate,
                        pd.receiptNo         AS ReceiptNo

                    FROM dbo.Reservation_Hed h
                    LEFT JOIN dbo.Reservation_RoomDetails_Det rd
                        ON rd.ReservationNo = h.ReservationNo
                    AND ISNULL(rd.IsDelete,0) = 0
                    LEFT JOIN dbo.Reservation_Service_Det sd
                        ON sd.ReservationNo = h.ReservationNo
                    AND ISNULL(sd.IsDelete,0) = 0
                    LEFT JOIN dbo.Reservation_Payment_Det pd
                        ON pd.ReservationNo = h.ReservationNo
                    ORDER BY h.ReservationDate DESC, h.ReservationNo;";

        using var cmd = new SqlCommand(sql, conn);
        if (top.HasValue) cmd.Parameters.Add("@top", SqlDbType.Int).Value = top.Value;

        var map = new Dictionary<string, ReservationDto>(StringComparer.OrdinalIgnoreCase);

        await conn.OpenAsync();
        using var rdr = await cmd.ExecuteReaderAsync();

        while (await rdr.ReadAsync())
        {
            // Hed (master)
            var resNo = rdr["ReservationNo"] as string ?? "";

            if (!map.TryGetValue(resNo, out var dto))
            {
                dto = new ReservationDto
                {
                    ReservationNo       = resNo,
                    ReservationDate     = rdr.GetDateTime(rdr.GetOrdinal("ReservationDate")),
                    ReservationType     = rdr.GetInt32(rdr.GetOrdinal("ReservationType")),
                    CustomerCode        = rdr["CustomerCode"] as string ?? "",
                    Mobile              = rdr["Mobile"] as string,
                    Telephone           = rdr["Telephone"] as string,
                    Email               = rdr["email"] as string,
                    TravelAgentCode     = rdr["TravelAgentCode"] as string,
                    CheckinDateTime     = rdr.GetDateTime(rdr.GetOrdinal("Checkindatetime")),
                    CheckoutDateTime    = rdr.GetDateTime(rdr.GetOrdinal("checkoutdatetime")),
                    NoOfVehicles        = rdr.GetInt32(rdr.GetOrdinal("noofVehicles")),
                    NoOfAdults          = rdr.GetInt32(rdr.GetOrdinal("noofadults")),
                    NoOfKids            = rdr.GetInt32(rdr.GetOrdinal("noofKids")),
                    EventType           = rdr["eventtype"] as string,
                    SetupStyle          = rdr["setupstyle"] as string,
                    SubTotal            = rdr.GetDecimal(rdr.GetOrdinal("SubTotal")),
                    DiscountPer         = rdr.GetDecimal(rdr.GetOrdinal("DiscountPer")),
                    Discount            = rdr.GetDecimal(rdr.GetOrdinal("Discount")),
                    GrossAmount         = rdr.GetDecimal(rdr.GetOrdinal("GrossAmount")),
                    PaidAmount          = rdr.GetDecimal(rdr.GetOrdinal("PaidAmount")),
                    DueAmount           = rdr.GetDecimal(rdr.GetOrdinal("DueAmount")),
                    ReservationNote     = rdr["Remark"] as string,
                    RefundAmount        = rdr.GetDecimal(rdr.GetOrdinal("RefundAmount")),
                    RefundNote          = rdr["refundnote"] as string,
                    ReferenceNo         = rdr["ReferenceReservationNo"] as string,
                    BookingResourceId   = rdr["BookingResourceId"] is DBNull ? 0 : rdr.GetInt32(rdr.GetOrdinal("BookingResourceId")),
                    BookingReferenceNo  = rdr["BookingReference"] as string,
                    ReservationStatus   = rdr["ReservationStatus"] as string,
                    User                = rdr["crUser"] as string,
                    RoomDetails         = new List<RoomDetailDto>(),
                    ServiceDetails      = new List<ServiceDetailDto>(),
                    RoomPayDetails      = new List<RoomPaymentDetailDto>()
                };

                map.Add(resNo, dto);
            }

            // RoomDetails (if present)
            if (!(rdr["RoomCode"] is DBNull) && !(rdr["PackageCode"] is DBNull))
            {
                dto.RoomDetails!.Add(new RoomDetailDto
                {
                    ReservationRoomDetailsID = rdr["ReservationRoomDetailsID"] is DBNull ? 0 : Convert.ToInt32(rdr["ReservationRoomDetailsID"]),
                    ReservationNo            = resNo,
                    RoomCode                 = rdr["RoomCode"] as string,
                    PackageCode              = rdr["PackageCode"] as string,
                    NoOfDays                 = rdr["noofdays"] is DBNull ? 0 : Convert.ToInt32(rdr["noofdays"]),
                    Price                    = rdr["rdPrice"]  is DBNull ? 0m : (decimal)rdr["rdPrice"],
                    Amount                   = rdr["rdAmount"] is DBNull ? 0m : (decimal)rdr["rdAmount"],
                    IsDelete                 = rdr["IsDelete"] is DBNull ? false : Convert.ToBoolean(rdr["IsDelete"]),
                    ModifiedDate             = rdr["ModifiedDate"] is DBNull ? DateTime.MinValue : (DateTime)rdr["ModifiedDate"],
                    CheckinDate              = rdr["checkindate"] is DBNull ? DateTime.MinValue : (DateTime)rdr["checkindate"],
                    CheckoutDate             = rdr["checkoutdate"] is DBNull ? DateTime.MinValue : (DateTime)rdr["checkoutdate"]
                });
            }

            // ServiceDetails (if present)
            if (!(rdr["ServiceTypeCode"] is DBNull))
            {
                dto.ServiceDetails!.Add(new ServiceDetailDto
                {
                    ServiceTypeCode   = rdr["ServiceTypeCode"] as string,
                    ServiceDate       = rdr["ServiceDate"] is DBNull ? DateTime.MinValue : (DateTime)rdr["ServiceDate"],
                    ServiceQuantity   = rdr["ServiceQuantity"] is DBNull ? 0 : Convert.ToInt32(rdr["ServiceQuantity"]),
                    ServiceAmount     = rdr["ServiceAmount"]   is DBNull ? 0m : (decimal)rdr["ServiceAmount"],
                    ServiceTotalAmount= rdr["ServiceTotalAmount"] is DBNull ? 0m : (decimal)rdr["ServiceTotalAmount"],
                    ServiceRemark     = rdr["serviceremark"] as string
                });
            }

            // PaymentDetails (if present)
            if (!(rdr["PaymentId"] is DBNull) || !(rdr["ReceiptNo"] is DBNull))
            {
                dto.RoomPayDetails!.Add(new RoomPaymentDetailDto
                {
                    PaymentId  = rdr["PaymentId"]  is DBNull ? 0  : Convert.ToInt32(rdr["PaymentId"]),
                    Amount     = rdr["PayAmount"]  is DBNull ? 0m : (decimal)rdr["PayAmount"],
                    RefNo      = rdr["RefNo"] as string,
                    RefDate    = rdr["RefDate"] is DBNull ? (DateTime?)null : (DateTime)rdr["RefDate"],
                    ReceiptNo  = rdr["ReceiptNo"] as string
                });
            }
        }

        return map.Values
                .OrderByDescending(r => r.ReservationDate)
                .ThenBy(r => r.ReservationNo)
                .ToList();
    }

    public async Task<string> SaveOrUpdateReservationAsync(ReservationDto dto)
    {
        using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        using var cmd = new SqlCommand("sp_reservation_save", conn) { CommandType = CommandType.StoredProcedure };

        try
        {
            AddVarchar(cmd, "@ReservationNo", 300, (dto.ReservationNo ?? "").Trim());
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
            AddVarchar(cmd, "@RefundNote", 100, string.IsNullOrWhiteSpace(dto.RefundNote) ? "" : dto.RefundNote);

            AddVarcharNullable(cmd, "@ReferenceNo", 50, dto.ReferenceNo);

            AddInt(cmd, "@BookingResourceId", dto.BookingResourceId);
            AddVarcharNullable(cmd, "@BookingReferenceNo", 50, dto.BookingReferenceNo);
            AddVarchar(cmd, "@ReservationStatus", 100,
            string.IsNullOrWhiteSpace(dto.ReservationStatus)
                ? throw new ArgumentException("ReservationStatus is required.")
                : dto.ReservationStatus);

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
                ToDbDate(x.CheckinDate),
                ToDbDate(x.CheckoutDate)
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
                ToDbDate(x.ServiceDate),
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
                ToDbDate(x.RefDate)
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
