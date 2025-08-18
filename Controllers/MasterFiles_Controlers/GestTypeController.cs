using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OIT_Reservation.Models;
using OIT_Reservation.Services;



namespace OIT_Reservation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerTypeController : ControllerBase
    {
        private readonly CustomerTypeService _service;

        public CustomerTypeController(CustomerTypeService service)
        {

            _service = service;
        }

        [HttpGet("getall")]
        public ActionResult<List<CustomerType>> CustomerTypes()
        {
            try
            {
                var customerTypes = _service.AllCustomerTypes();
                return Ok(customerTypes);
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