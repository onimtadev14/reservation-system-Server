using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OIT_Reservation.Services;

namespace OIT_Reservation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CountryController : ControllerBase
    {

        private readonly CountryService _service;
        public CountryController(CountryService service)
        {
            _service = service;
        }


        //GET: api/country
        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            try
            {
                var result = _service.GetAllCountries();
                return Ok(result);
            }
            catch (SqlException ex)
            {
                return BadRequest($"SQL Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }
    }
}
  
