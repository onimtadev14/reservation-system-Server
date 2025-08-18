
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;


[ApiController]
[Route("api/[controller]")]
public class TitleController : ControllerBase
{

    private readonly TitleService _service;

    public TitleController(TitleService service)
    {
        _service = service;
    }

    [HttpGet("getall")]
    public IActionResult AllTitles()
    {
        try
        {
            var titles = _service.GetTitles();
            Console.WriteLine("Titles retrieved successfully.");
            return Ok(titles);
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


