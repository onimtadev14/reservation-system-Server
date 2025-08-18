using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OIT_Reservation.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace OIT_Reservation.Services
{
    public class CustomerTypeService
    {
        private readonly string _conn;

        public CustomerTypeService(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection");
        }

        public List<CustomerType> AllCustomerTypes()
        {
            var list = new List<CustomerType>();

            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_GetAllCustomerTypes", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new CustomerType
                {
                    CustomerTypeID = Convert.ToInt32(reader["CustomerTypeID"]),
                    CustomerTypeCode = reader["CustomerTypeCode"].ToString(),
                    Description = reader["Description"].ToString()
                });
            }

            return list;
        }
    }
}