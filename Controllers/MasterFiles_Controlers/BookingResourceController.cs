using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OIT_Reservation.Services;
using YourNamespace.Services;

namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingResourceController : ControllerBase
    {
        private readonly BookingResourceService _service;
        public BookingResourceController(BookingResourceService service)
        {
            _service = service;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var resources = await _service.GetAllBookingResourcesAsync();
                return Ok(resources);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch booking resources", error = ex.Message });
            }
        }
    }
}
