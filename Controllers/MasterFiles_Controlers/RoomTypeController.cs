using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OIT_Reservation.Models;
using OIT_Reservation.Services;

namespace OIT_Reservation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomTypeController : ControllerBase
    {
        private readonly RoomTypeService _service;

        public RoomTypeController(RoomTypeService service)
        {
            _service = service;
        }

        [HttpGet("getNextCode")]

        public IActionResult GetNextCode()
        {
            try
            {
                var roomType = _service.GetNextCode();
                return Ok(new { nextCode = roomType.RoomTypeCode });
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

        //GET: api/roomtype
        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            var result = _service.GetAll();
            return Ok(result);
        }

        // POST: api/roomtype[HttpPost]
        [HttpPost("add")]
        public IActionResult Create([FromBody] RoomType roomType)
        {
            try
            {
                bool success = _service.Create(roomType);
                return Ok(new
                {
                    message = "Room type created successfully.",
                    generatedCode = roomType.RoomTypeCode
                });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }


        [HttpPut("Update/{id}")]
        public IActionResult Update(long id, [FromBody] RoomType roomType)
        {
            try
            {
                roomType.RoomTypeID = id;

                bool updated = _service.Update(roomType);
                if (updated)
                    return Ok("Room type updated successfully.");
                else
                    return NotFound("Room type not found.");
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
