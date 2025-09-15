public class PayType
{
    public int PaymentID { get; set; }
    public string Descrip { get; set; }
    public bool IsSwipe { get; set; }
    public string Type { get; set; }
    public decimal? Rate { get; set; }
    public bool IsRefundable { get; set; }
    public bool IsActive { get; set; }
    public bool IsBillCopy { get; set; }
    public string PrintDescrip { get; set; }
    public string PreFix { get; set; }
    public int? MaxLength { get; set; }
    public int? OrderNo { get; set; }
    public bool SignatureOnPrint { get; set; }
}
