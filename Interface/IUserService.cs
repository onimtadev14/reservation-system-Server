using OIT_Reservation.Model;

namespace OIT_Reservation.Interface
{
    public interface IUserService
    {
        bool ValidateUser(User user);
        bool RegisterUser(User user); // Add this method to the interface
    }
}