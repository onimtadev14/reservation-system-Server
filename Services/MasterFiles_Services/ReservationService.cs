using OIT_Reservation.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;

namespace OIT_Reservation.Services
{
    public class ReservationService : IReservationService
    {
        private readonly string _conn;

        public ReservationService(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection");
        }

        



        public List<Reservation> GetAll()
        {
            var list = new List<Reservation>();
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_GetAllReservations", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Reservation
                {
                    Id = (int)reader["Id"],
                    ReservedBy = reader["ReservedBy"].ToString(),
                    Date = (DateTime)reader["Date"],
                    Purpose = reader["Purpose"].ToString()
                });
            }
            return list;
        }

        public Reservation GetById(int id)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_GetReservationById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Id", id);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Reservation
                {
                    Id = (int)reader["Id"],
                    ReservedBy = reader["ReservedBy"].ToString(),
                    Date = (DateTime)reader["Date"],
                    Purpose = reader["Purpose"].ToString()
                };
            }
            return null;
        }

        public bool Create(Reservation reservation)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_CreateReservation", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@ReservedBy", reservation.ReservedBy);
            cmd.Parameters.AddWithValue("@Date", reservation.Date);
            cmd.Parameters.AddWithValue("@Purpose", reservation.Purpose);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Update(Reservation reservation)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_UpdateReservation", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Id", reservation.Id);
            cmd.Parameters.AddWithValue("@ReservedBy", reservation.ReservedBy);
            cmd.Parameters.AddWithValue("@Date", reservation.Date);
            cmd.Parameters.AddWithValue("@Purpose", reservation.Purpose);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_DeleteReservation", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Id", id);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
