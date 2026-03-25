using Contract.Messages;
using MassTransit;
using OrderService.Application.Commands.PlaceOrder;
using OrderService.Application.Common.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Data;
using Payment.Proto;

namespace OrderService.Application.Handler.PlaceOrder
{
    public class PlaceOrderHandler
    {
        private readonly PaymentGrpc.PaymentGrpcClient _paymentClient;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IOrderDbContext _db;

        public PlaceOrderHandler(
            PaymentGrpc.PaymentGrpcClient paymentClient,
            IPublishEndpoint publishEndpoint,
            IOrderDbContext db)
        {
            _paymentClient = paymentClient;
            _publishEndpoint = publishEndpoint;
            _db = db;
        }

        public async Task<int> Handle(PlaceOrderCommand command)
        {
            var orderId = new Random().Next(1000, 9999);

            var order = new Order(orderId, command.Amount);

            Console.WriteLine("➡️ Calling Payment...");

            var payment = await _paymentClient.ProcessPaymentAsync(
                new PaymentRequest
                {
                    OrderId = order.Id,
                    Amount = order.Amount
                });

            if (!payment.Success)
                throw new Exception("Payment failed");

            Console.WriteLine("✅ Payment successful");

            // ✅ SAVE TO DATABASE
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            Console.WriteLine("💾 Order saved to DB");

            Console.WriteLine("➡️ Publishing event...");

            await _publishEndpoint.Publish(new OrderPlaced
            {
                OrderId = order.Id,
                Amount = order.Amount
            });

            Console.WriteLine("✅ Event published");

            return order.Id;
        }
    }
}
