using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

[Route("api/[controller]")]
[ApiController]
public class NationalityController : ControllerBase
{
    private readonly NationalityService _service;

    public NationalityController(IConfiguration config)
    {
        _service = new NationalityService(config);
    }

    [HttpGet("getall")]
    public IActionResult GetAllNationalities()
    {
        try
        {
            var data = _service.GetAllNationalities();
            return Ok(data);
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
