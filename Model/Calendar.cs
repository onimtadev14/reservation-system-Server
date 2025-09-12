public class ReservationCalendarDto
{
    public int RoomId { get; set; }
    public string RoomName { get; set; }
    public string RoomSize { get; set; }

    // Dynamic date → reservation detail mapping
    public Dictionary<string, string> CalendarDetails { get; set; } = new();
}
