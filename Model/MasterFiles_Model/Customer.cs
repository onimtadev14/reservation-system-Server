public class Customer
{
    public string CustomerTypeCode { get; set; }
    public string CustomerCode { get; set; } = "";
    public string Title { get; set; }
    public string Name { get; set; }
    public string NIC_PassportNo { get; set; }
    public string NationalityCode { get; set; }
    public string CountryCode { get; set; }
    public string Mobile { get; set; }
    public string Telephone { get; set; }
    public string Email { get; set; }
    public string TravelAgentCode { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; }
    public bool IsNew { get; set; }  // true = insert, false = update
    public string Address { get; set; }
    public string Whatsapp { get; set; }
    public string Remark { get; set; }
}
