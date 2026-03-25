using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PaymentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Payment Service API working");
        }
    }
}
