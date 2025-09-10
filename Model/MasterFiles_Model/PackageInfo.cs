public class PackageInfo
{
    public long PackageID { get; set; }
    public string PackageCode { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public decimal PackageDuration { get; set; }
    public string? Remarks { get; set; }

    public decimal RoomPrice { get; set; }
    public decimal RoomCost { get; set; }
    public decimal RoomAmount { get; set; }
    public decimal FoodAmount { get; set; }
    public decimal BeverageAmount { get; set; }

    public bool IsRoom { get; set; }
    public bool IsBanquet { get; set; }
    public bool IsVilla { get; set; }


    public string Type => IsRoom ? "Room" : (IsBanquet ? "Banquet" : (IsVilla ? "Villa" : "Unknown"));
}
