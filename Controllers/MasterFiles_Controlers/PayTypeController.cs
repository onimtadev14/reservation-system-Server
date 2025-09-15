using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

[ApiController]
[Route("api/[controller]")]
public class PayTypeController : ControllerBase
{
    private readonly IPayTypeService _payTypeService;

    public PayTypeController(IPayTypeService payTypeService)
    {
        _payTypeService = payTypeService;
    }

    [HttpGet("getall")]
    public ActionResult<IEnumerable<PayType>> GetActivePayTypes()
    {
        var result = _payTypeService.GetActivePayTypes();
        return Ok(result);
    }
}
