using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public interface IPayTypeService
{
    IEnumerable<PayType> GetActivePayTypes();
}

public class PayTypeService : IPayTypeService
{
    private readonly string _conn;

    public PayTypeService(IConfiguration config)
    {
        _conn = config.GetConnectionString("DefaultConnection");
    }

    public IEnumerable<PayType> GetActivePayTypes()
    {
        var payTypes = new List<PayType>();

        using (SqlConnection conn = new SqlConnection(_conn))
        using (SqlCommand cmd = new SqlCommand("sp_GetAllPayTypes", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            conn.Open();

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    payTypes.Add(new PayType
                    {
                        PaymentID = reader.GetInt32(reader.GetOrdinal("PaymentID")),
                        Descrip = reader["Descrip"].ToString(),
                        IsSwipe = reader.GetBoolean(reader.GetOrdinal("IsSwipe")),
                        Type = reader["Type"].ToString(),
                        Rate = reader["Rate"] as decimal?,
                        IsRefundable = reader.GetBoolean(reader.GetOrdinal("IsRefundable")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                        IsBillCopy = reader.GetBoolean(reader.GetOrdinal("IsBillCopy")),
                        PrintDescrip = reader["PrintDescrip"].ToString(),
                        PreFix = reader["PreFix"].ToString(),
                        MaxLength = reader["MaxLength"] as int?,
                        OrderNo = reader["OrderNo"] as int?,
                        SignatureOnPrint = reader.GetBoolean(reader.GetOrdinal("SignatureOnPrint"))
                    });
                }
            }
        }

        return payTypes;
    }
}
