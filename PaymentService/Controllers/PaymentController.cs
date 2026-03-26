using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Handler.GetOrders;
using OrderService.Application.Queries.GetOrders;

namespace PaymentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly GetPaymentsHandler _getPaymentsHandler;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(GetPaymentsHandler getPaymentsHandler, ILogger<PaymentController> logger)
        {
            _getPaymentsHandler = getPaymentsHandler;
            _logger = logger;
        }


        [HttpGet]
        public async Task<IActionResult> GetPayments()
        {
            _logger.LogInformation("📥 GET /api/payment called");

            var payments = await _getPaymentsHandler.Handle(new GetPaymentsQuery());
            return Ok(payments);
        }
    }
}
