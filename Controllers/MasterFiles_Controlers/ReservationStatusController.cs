using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

[ApiController]
[Route("api/[controller]")]
public class ReservationStatusController : ControllerBase
{
    private readonly IReservationStatusService _reservationStatusService;

    public ReservationStatusController(IReservationStatusService reservationStatusService)
    {
        _reservationStatusService = reservationStatusService;
    }

    [HttpGet("getall")]
    public ActionResult<IEnumerable<ReservationStatus>> GetVisibleStatuses()
    {
        var result = _reservationStatusService.GetVisibleStatuses();
        return Ok(result);
    }
}
