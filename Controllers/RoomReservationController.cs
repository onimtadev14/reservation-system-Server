using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class RoomReservationController : ControllerBase
{
    private readonly IRoomReservationService _reservationService;
    private readonly ILogger<RoomReservationController> _logger;

    public RoomReservationController(
        IRoomReservationService reservationService,
        ILogger<RoomReservationController> logger)
    {
        _reservationService = reservationService;
        _logger = logger;
    }

    [HttpPost("save")]
    public async Task<IActionResult> SaveReservation([FromBody] ReservationDto reservation)
    {
        if (reservation == null)
        {
            _logger.LogWarning("Invalid reservation data received");
            return BadRequest(new { message = "Invalid reservation data" });
        }

        try
        {
            _logger.LogInformation("Saving reservation for customer: {CustomerCode}", reservation.CustomerCode);

            var reservationNo = await _reservationService.SaveOrUpdateReservationAsync(reservation);

            _logger.LogInformation("Reservation saved successfully: {ReservationNo}", reservationNo);

            return Ok(new
            {
                ReservationNo = reservationNo,
                Message = "Reservation saved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving reservation for customer: {CustomerCode}", reservation.CustomerCode);

            return StatusCode(500, new
            {
                message = "An error occurred while saving the reservation",
                error = ex.Message
            });
        }
    }


    // NEW: GET ALL
    [HttpGet("all")]
    public async Task<IActionResult> GetAll([FromQuery] int? top = null)
    {
        try
        {
            var data = await _reservationService.GetAllAsync(top);
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all reservations.");
            return StatusCode(500, "An error occurred while retrieving reservations: " + ex.Message);
        }

    }
}
