using Contract.Messages;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Commands.PlaceOrder;
using OrderService.Application.Handler.PlaceOrder;
using Payment.Proto;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly PlaceOrderHandler _handler;

        public OrderController(PlaceOrderHandler handler)
        {
            _handler = handler;
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderCommand command)
        {
            var orderId = await _handler.Handle(command);

            return Ok($"Order {orderId} placed");
        }
    }
}
