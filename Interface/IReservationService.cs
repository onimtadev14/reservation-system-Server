using OIT_Reservation.Models;
using System.Collections.Generic;

namespace OIT_Reservation.Services
{
    public interface IReservationService
    {
        List<Reservation> GetAll();
        Reservation GetById(int id);
        bool Create(Reservation reservation);
        bool Update(Reservation reservation);
        bool Delete(int id);
    }
}
