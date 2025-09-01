using Microsoft.Data.SqlClient;
using System.Data;

public interface IRoomReservationService
{
    Task<string> SaveOrUpdateReservationAsync(ReservationDto dto);
     Task<IReadOnlyList<ReservationDto>> GetAllReservationsAsync(int? top = null);

}





public class RoomReservationService : IRoomReservationService
{
    private readonly IConfiguration _config;
    public RoomReservationService(IConfiguration config) => _config = config;

    public async Task<IReadOnlyList<ReservationDto>> GetAllReservationsAsync(int? top = null)
    {
        using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        using var cmd  = new SqlCommand("sp_reservation_get_all", conn) { CommandType = CommandType.StoredProcedure };
        if (top.HasValue)
        {
            var pTop = cmd.Parameters.Add("@Top", SqlDbType.Int);
            pTop.Value = top.Value;
        }

        var map = new Dictionary<string, ReservationDto>(StringComparer.OrdinalIgnoreCase);

        // to prevent duplicates caused by LEFT JOINs (rooms x services x payments)
        var roomSeen    = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var serviceSeen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var paySeen     = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        await conn.OpenAsync();
        using var rdr = await cmd.ExecuteReaderAsync();

        // Get ordinals once (assumes column names exactly as in sp_reservation_get_all I gave you)
        int oReservationNo       = rdr.GetOrdinal("ReservationNo");
        int oReservationDate     = rdr.GetOrdinal("ReservationDate");
        int oReservationType     = rdr.GetOrdinal("ReservationType");
        int oCustomerCode        = rdr.GetOrdinal("CustomerCode");
        int oMobile              = rdr.GetOrdinal("Mobile");
        int oTelephone           = rdr.GetOrdinal("Telephone");
        int oEmail               = rdr.GetOrdinal("Email");
        int oTravelAgentCode     = rdr.GetOrdinal("TravelAgentCode");
        int oCheckinDateTime     = rdr.GetOrdinal("CheckinDateTime");
        int oCheckoutDateTime    = rdr.GetOrdinal("CheckoutDateTime");
        int oNoOfVehicles        = rdr.GetOrdinal("NoOfVehicles");
        int oNoOfAdults          = rdr.GetOrdinal("NoOfAdults");
        int oNoOfKids            = rdr.GetOrdinal("NoOfKids");
        int oEventType           = rdr.GetOrdinal("EventType");
        int oSetupStyle          = rdr.GetOrdinal("SetupStyle");
        int oSubTotal            = rdr.GetOrdinal("SubTotal");
        int oDiscountPer         = rdr.GetOrdinal("DiscountPer");
        int oDiscount            = rdr.GetOrdinal("Discount");
        int oGrossAmount         = rdr.GetOrdinal("GrossAmount");
        int oPaidAmount          = rdr.GetOrdinal("PaidAmount");
        int oDueAmount           = rdr.GetOrdinal("DueAmount");
        int oReservationNote     = rdr.GetOrdinal("ReservationNote");
        int oRefundAmount        = rdr.GetOrdinal("RefundAmount");
        int oRefundNote          = rdr.GetOrdinal("RefundNote");
        int oReferenceNo         = rdr.GetOrdinal("ReferenceNo");
        int oBookingResourceId   = rdr.GetOrdinal("BookingResourceId");
        int oBookingReferenceNo  = rdr.GetOrdinal("BookingReferenceNo");
        int oReservationStatus   = rdr.GetOrdinal("ReservationStatus");
        int oUser                = rdr.GetOrdinal("User");

        // Room detail columns (may be NULL)
        int oResRoomDetailsID = rdr.GetOrdinal("ReservationRoomDetailsID");
        int oRoomCode         = rdr.GetOrdinal("RoomCode");
        int oPackageCode      = rdr.GetOrdinal("PackageCode");
        int oNoOfDays         = rdr.GetOrdinal("NoOfDays");
        int oPrice            = rdr.GetOrdinal("Price");
        int oAmount           = rdr.GetOrdinal("Amount");
        int oIsDelete         = rdr.GetOrdinal("IsDelete");
        int oModifiedDate     = rdr.GetOrdinal("ModifiedDate");
        int oCheckinDate      = rdr.GetOrdinal("CheckinDate");
        int oCheckoutDate     = rdr.GetOrdinal("CheckoutDate");

        // Service detail columns (may be NULL)
        int oSvcTypeCode      = rdr.GetOrdinal("ServiceTypeCode");
        int oSvcDate          = rdr.GetOrdinal("ServiceDate");
        int oSvcQty           = rdr.GetOrdinal("ServiceQuantity");
        int oSvcAmount        = rdr.GetOrdinal("ServiceAmount");
        int oSvcTotalAmount   = rdr.GetOrdinal("ServiceTotalAmount");
        int oSvcRemark        = rdr.GetOrdinal("ServiceRemark");

        // Payment detail columns (may be NULL)
        int oPayPaymentId     = rdr.GetOrdinal("PaymentId");
        int oPayAmount        = rdr.GetOrdinal("PayAmount");
        int oPayRefNo         = rdr.GetOrdinal("RefNo");
        int oPayRefDate       = rdr.GetOrdinal("RefDate");
        int oPayReceiptNo     = rdr.GetOrdinal("ReceiptNo");

        while (await rdr.ReadAsync())
        {
            var resNo = rdr.GetString(oReservationNo);

            if (!map.TryGetValue(resNo, out var dto))
            {
                dto = new ReservationDto
                {
                    ReservationNo     = resNo,
                    ReservationDate   = rdr.GetDateTime(oReservationDate),
                    ReservationType   = rdr.GetInt32(oReservationType),
                    CustomerCode      = rdr.GetString(oCustomerCode),
                    Mobile            = rdr.IsDBNull(oMobile) ? null : rdr.GetString(oMobile),
                    Telephone         = rdr.IsDBNull(oTelephone) ? null : rdr.GetString(oTelephone),
                    Email             = rdr.IsDBNull(oEmail) ? null : rdr.GetString(oEmail),
                    TravelAgentCode   = rdr.IsDBNull(oTravelAgentCode) ? null : rdr.GetString(oTravelAgentCode),
                    CheckinDateTime   = rdr.GetDateTime(oCheckinDateTime),
                    CheckoutDateTime  = rdr.GetDateTime(oCheckoutDateTime),
                    NoOfVehicles      = rdr.GetInt32(oNoOfVehicles),
                    NoOfAdults        = rdr.GetInt32(oNoOfAdults),
                    NoOfKids          = rdr.GetInt32(oNoOfKids),
                    EventType         = rdr.IsDBNull(oEventType) ? null : rdr.GetString(oEventType),
                    SetupStyle        = rdr.IsDBNull(oSetupStyle) ? null : rdr.GetString(oSetupStyle),
                    SubTotal          = rdr.GetDecimal(oSubTotal),
                    DiscountPer       = rdr.GetDecimal(oDiscountPer),
                    Discount          = rdr.GetDecimal(oDiscount),
                    GrossAmount       = rdr.GetDecimal(oGrossAmount),
                    PaidAmount        = rdr.GetDecimal(oPaidAmount),
                    DueAmount         = rdr.GetDecimal(oDueAmount),
                    ReservationNote   = rdr.IsDBNull(oReservationNote) ? null : rdr.GetString(oReservationNote),
                    RefundAmount      = rdr.GetDecimal(oRefundAmount),
                    RefundNote        = rdr.IsDBNull(oRefundNote) ? null : rdr.GetString(oRefundNote),
                    ReferenceNo       = rdr.IsDBNull(oReferenceNo) ? null : rdr.GetString(oReferenceNo),
                    BookingResourceId = rdr.GetInt32(oBookingResourceId),
                    BookingReferenceNo= rdr.IsDBNull(oBookingReferenceNo) ? null : rdr.GetString(oBookingReferenceNo),
                    ReservationStatus = rdr.IsDBNull(oReservationStatus) ? null : rdr.GetString(oReservationStatus),
                    User              = rdr.IsDBNull(oUser) ? null : rdr.GetString(oUser),

                    RoomDetails       = new List<RoomDetailDto>(),
                    ServiceDetails    = new List<ServiceDetailDto>(),
                    RoomPayDetails    = new List<RoomPaymentDetailDto>()
                };
                map[resNo] = dto;
            }

            // ---- Room detail (guard against NULL row) ----
            if (!rdr.IsDBNull(oRoomCode) || !rdr.IsDBNull(oPackageCode) || !rdr.IsDBNull(oResRoomDetailsID))
            {
                var rk = $"{resNo}|{(rdr.IsDBNull(oRoomCode) ? "" : rdr.GetString(oRoomCode))}|{(rdr.IsDBNull(oPackageCode) ? "" : rdr.GetString(oPackageCode))}|{(rdr.IsDBNull(oResRoomDetailsID) ? 0 : rdr.GetInt64(oResRoomDetailsID))}";
                if (roomSeen.Add(rk))
                {
                    dto.RoomDetails!.Add(new RoomDetailDto
                    {
                        ReservationRoomDetailsID = rdr.IsDBNull(oResRoomDetailsID) ? 0 : rdr.GetInt32(oResRoomDetailsID),
                        ReservationNo            = resNo,
                        RoomCode                 = rdr.IsDBNull(oRoomCode) ? null : rdr.GetString(oRoomCode),
                        PackageCode              = rdr.IsDBNull(oPackageCode) ? null : rdr.GetString(oPackageCode),
                        NoOfDays                 = rdr.IsDBNull(oNoOfDays) ? 0 : rdr.GetInt32(oNoOfDays),
                        Price                    = rdr.IsDBNull(oPrice) ? 0m : rdr.GetDecimal(oPrice),
                        Amount                   = rdr.IsDBNull(oAmount) ? 0m : rdr.GetDecimal(oAmount),
                        IsDelete                 = rdr.IsDBNull(oIsDelete) ? false : rdr.GetBoolean(oIsDelete),
                        ModifiedDate             = rdr.IsDBNull(oModifiedDate) ? default : rdr.GetDateTime(oModifiedDate),
                        CheckinDate              = rdr.IsDBNull(oCheckinDate) ? default : rdr.GetDateTime(oCheckinDate),
                        CheckoutDate             = rdr.IsDBNull(oCheckoutDate) ? default : rdr.GetDateTime(oCheckoutDate)
                    });
                }
            }

            // ---- Service detail ----
            if (!rdr.IsDBNull(oSvcTypeCode) || !rdr.IsDBNull(oSvcDate))
            {
                var sk = $"{resNo}|{(rdr.IsDBNull(oSvcTypeCode) ? "" : rdr.GetString(oSvcTypeCode))}|{(rdr.IsDBNull(oSvcDate) ? DateTime.MinValue : rdr.GetDateTime(oSvcDate))}";
                if (serviceSeen.Add(sk))
                {
                    dto.ServiceDetails!.Add(new ServiceDetailDto
                    {
                        ServiceTypeCode   = rdr.IsDBNull(oSvcTypeCode) ? null : rdr.GetString(oSvcTypeCode),
                        ServiceDate       = rdr.IsDBNull(oSvcDate) ? default : rdr.GetDateTime(oSvcDate),
                        ServiceQuantity   = rdr.IsDBNull(oSvcQty) ? 0 : rdr.GetInt32(oSvcQty),
                        ServiceAmount     = rdr.IsDBNull(oSvcAmount) ? 0m : rdr.GetDecimal(oSvcAmount),
                        ServiceTotalAmount= rdr.IsDBNull(oSvcTotalAmount) ? 0m : rdr.GetDecimal(oSvcTotalAmount),
                        ServiceRemark     = rdr.IsDBNull(oSvcRemark) ? null : rdr.GetString(oSvcRemark)
                    });
                }
            }

            // ---- Payment detail ----
            if (!rdr.IsDBNull(oPayPaymentId) || !rdr.IsDBNull(oPayReceiptNo))
            {
                var pk = $"{resNo}|{(rdr.IsDBNull(oPayReceiptNo) ? "" : rdr.GetString(oPayReceiptNo))}|{(rdr.IsDBNull(oPayPaymentId) ? 0 : rdr.GetInt64(oPayPaymentId))}|{(rdr.IsDBNull(oPayRefNo) ? "" : rdr.GetString(oPayRefNo))}|{(rdr.IsDBNull(oPayRefDate) ? DateTime.MinValue : rdr.GetDateTime(oPayRefDate))}";
                if (paySeen.Add(pk))
                {
                    dto.RoomPayDetails!.Add(new RoomPaymentDetailDto
                    {
                        PaymentId = rdr.IsDBNull(oPayPaymentId) ? 0 : rdr.GetInt32(oPayPaymentId),
                        Amount    = rdr.IsDBNull(oPayAmount) ? 0m : rdr.GetDecimal(oPayAmount),
                        RefNo     = rdr.IsDBNull(oPayRefNo) ? null : rdr.GetString(oPayRefNo),
                        RefDate   = rdr.IsDBNull(oPayRefDate) ? (DateTime?)null : rdr.GetDateTime(oPayRefDate),
                        ReceiptNo = rdr.IsDBNull(oPayReceiptNo) ? null : rdr.GetString(oPayReceiptNo)
                    });
                }
            }
        }

        return map.Values.OrderByDescending(x => x.ReservationDate).ThenBy(x => x.ReservationNo).ToList();
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
