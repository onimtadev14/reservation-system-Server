public class SetupStyle
{
    public long SetupStyleTypeID { get; set; }
    public string SetupStyleCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public bool IsActive { get; set; } = true;
}