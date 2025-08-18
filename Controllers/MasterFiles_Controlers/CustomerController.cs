using Microsoft.AspNetCore.Mvc;
using OIT_Reservation.Services;

namespace OIT_Reservation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerService _customerService;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(CustomerService customerService, ILogger<CustomerController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        [HttpGet("getNextCode")]
        public IActionResult GetNextCode()
        {
            try
            {
                var nextCode = _customerService.GetNextCustomerCode();
                return Ok(new { nextCode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting next customer code.");
                return StatusCode(500, "An error occurred while retrieving the next customer code.");
            }
        }


        [HttpPost("save")]
        public IActionResult SaveCustomer([FromBody] Customer customer)
        {
            try
            {
                var code = _customerService.CustomerSave(customer);
                return Ok(new { CustomerCode = code });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while saving the customer. " + ex.Message);
            }
        }

        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            try
            {
                var list = _customerService.GetAllCustomers(); // NOTE: uses _service not _customerService
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving customers." + ex.Message);
            }
        }

        [HttpPut("update/{customerCode}")]
        public IActionResult UpdateCustomerByCode(string customerCode, [FromBody] Customer customer)
        {
            try
            {
                var updatedCode = _customerService.UpdateCustomerByCode(customerCode, customer);
                return Ok(new { UpdatedCustomerCode = updatedCode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer.");
                return StatusCode(500, "An error occurred while updating the customer.");
            }
        }



    }
}


