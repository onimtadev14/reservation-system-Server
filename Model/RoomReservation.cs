public class ReservationDto
{
    public string ReservationNo { get; set; } = "";
    public DateTime ReservationDate { get; set; }
    public int ReservationType { get; set; }
    public string CustomerCode { get; set; } = "";
    public string Mobile { get; set; } = "";
    public string Telephone { get; set; } = "";
    public string Email { get; set; } = "";
    public string TravelAgentCode { get; set; } = "";
    public DateTime CheckinDateTime { get; set; }
    public DateTime CheckoutDateTime { get; set; }
    public int NoOfVehicles { get; set; }
    public int NoOfAdults { get; set; }
    public int NoOfKids { get; set; }
    public string EventType { get; set; } = "";
    public string SetupStyle { get; set; } = "";
    public decimal SubTotal { get; set; }
    public decimal DiscountPer { get; set; }
    public decimal Discount { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal DueAmount { get; set; }
    public string ReservationNote { get; set; } = "";
    public decimal RefundAmount { get; set; }
    public string RefundNote { get; set; } = "";
    public string ReferenceNo { get; set; } = "";
    public int BookingResourceId { get; set; }
    public string BookingReferenceNo { get; set; } = "";
    public string ReservationStatus { get; set; } = "";
    public string User { get; set; } = "";

    // Child collections (TVPs)
    public List<RoomDetailDto> RoomDetails { get; set; } = new List<RoomDetailDto>();
    public List<ServiceDetailDto> ServiceDetails { get; set; } = new List<ServiceDetailDto>();
    public List<RoomPaymentDetailDto> RoomPayDetails { get; set; } = new List<RoomPaymentDetailDto>();
}

public class RoomDetailDto
{
    public string RoomCode { get; set; } = "";
    public string PackageCode { get; set; } = "";
    public int NoOfDays { get; set; }
    public decimal Price { get; set; }
    public decimal Amount { get; set; }
    public DateTime Checkin { get; set; }
    public DateTime Checkout { get; set; }
}

public class ServiceDetailDto
{
    public string ServiceTypeCode { get; set; } = "";
    public int ServiceQuantity { get; set; }
    public decimal ServiceAmount { get; set; }
    public decimal ServiceTotalAmount { get; set; }
    public DateTime ServiceDate { get; set; }
    public string ServiceRemark { get; set; } = "";
}

public class RoomPaymentDetailDto
{
    public int PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string RefNo { get; set; } = "";
    public DateTime? RefDate { get; set; }
    public string ReceiptNo { get; set; } = "";
}