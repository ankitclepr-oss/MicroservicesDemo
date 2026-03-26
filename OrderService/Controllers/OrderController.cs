using Contract.Messages;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Commands.PlaceOrder;
using OrderService.Application.Handler.GetOrders;
using OrderService.Application.Handler.PlaceOrder;
using OrderService.Application.Queries.GetOrders;
using Payment.Proto;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly PlaceOrderHandler _placeOrderHandler;
        private readonly GetOrdersHandler _getOrdersHandler;
        private readonly ILogger<OrderController> _logger;

        public OrderController(PlaceOrderHandler placeOrderHandler,GetOrdersHandler getOrdersHandler, ILogger<OrderController> logger   )
        {
            _placeOrderHandler = placeOrderHandler;
            _getOrdersHandler = getOrdersHandler;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderCommand command)
        {
            _logger.LogInformation("📥 POST /api/order called with Amount: {Amount}", command.Amount);

            var correlationId = HttpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault();

            var updatedCommand = command with
            {
                CorrelationId = correlationId
            };

            var orderId = await _placeOrderHandler.Handle(updatedCommand);

            return Ok($"Order {orderId} placed");
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            _logger.LogInformation("📥 GET /api/order called");

            var orders = await _getOrdersHandler.Handle(new GetOrdersQuery());
            return Ok(orders);
        }
    }
}
