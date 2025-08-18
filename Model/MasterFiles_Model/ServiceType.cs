public class ServiceType
{
    public long ServiceTypeID { get; set; }
    public string ServiceCode { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal ServiceAmount { get; set; }
    public string? Remarks { get; set; }
    public bool IsRoom { get; set; }
    public bool IsBanquet { get; set; }

    public string Type => IsRoom ? "Room" : (IsBanquet ? "Banquet" : "Unknown");
}
