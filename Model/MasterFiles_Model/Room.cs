public class Room
{
    public int RoomID { get; set; }
    public string RoomTypeCode { get; set; }
    public string RoomCode { get; set; }
    public string RoomName { get; set; }
    public int RoomSize { get; set; }
    public string RoomStatus { get; set; }
    public string Remarks { get; set; }
    public bool IsRoom { get; set; }
    public bool IsBanquet { get; set; }
    public object? Description { get; internal set; }
}
