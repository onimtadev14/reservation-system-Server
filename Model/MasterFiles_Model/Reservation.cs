namespace OIT_Reservation.Models
{
    public class Reservation
    {
        public long Id { get; set; }
        public string ReservedBy { get; set; }
        public DateTime Date { get; set; }
        public string Purpose { get; set; }
    }
}
