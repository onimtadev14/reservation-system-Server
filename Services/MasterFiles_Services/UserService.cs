using OIT_Reservation.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using OIT_Reservation.Interface;
using OIT_Reservation.Model;
using Microsoft.AspNetCore.Identity;


namespace OIT_Reservation.Services
{
    public class UserService : IUserService
    {
        private readonly string _conn;

        public UserService(IConfiguration config)
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            _conn = config.GetConnectionString("DefaultConnection");
#pragma warning restore CS8601 // Possible null reference assignment.
        }

public bool ValidateUser(User inputUser)
{
    using var conn = new SqlConnection(_conn);
    using var cmd = new SqlCommand("sp_LoginUser", conn)
    {
        CommandType = CommandType.StoredProcedure
    };

    cmd.Parameters.AddWithValue("@Username", inputUser.Username);

    conn.Open();
    using var reader = cmd.ExecuteReader(CommandBehavior.SingleRow);

    if (reader.Read())
    {
        string storedHash = reader["Password"].ToString()!;
        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(inputUser, storedHash, inputUser.Password);
        return result == PasswordVerificationResult.Success;
    }

    return false;
}


public bool RegisterUser(User user)
{
    

    // Hash the password before sending to SQL
    var hasher = new PasswordHasher<User>();
    string hashedPassword = hasher.HashPassword(user, user.Password);

    try
            {
                using (var conn = new SqlConnection(_conn))
                using (var cmd = new SqlCommand("sp_RegisterUser", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Username", user.Username);
                    cmd.Parameters.AddWithValue("@Password", hashedPassword);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                return true; // success
            }
            catch (SqlException ex)
            {
                // Capture SQL Server error message
                Console.WriteLine("SQL Error: " + ex.Message);
                return false; // failure
            }
}


    }
}
