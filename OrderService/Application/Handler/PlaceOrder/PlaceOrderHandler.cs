using Contract.Messages;
using Grpc.Core;
using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Commands.PlaceOrder;
using OrderService.Application.Common.Interfaces;
using OrderService.Domain.Entities;
using Payment.Proto;

namespace OrderService.Application.Handler.PlaceOrder
{
    public class PlaceOrderHandler
    {
        private readonly PaymentGrpc.PaymentGrpcClient _paymentClient;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IOrderDbContext _db;
        private readonly ILogger<PlaceOrderHandler> _logger;

        public PlaceOrderHandler(
            PaymentGrpc.PaymentGrpcClient paymentClient,
            IPublishEndpoint publishEndpoint,
            IOrderDbContext db,
            ILogger<PlaceOrderHandler> logger)
        {
            _paymentClient = paymentClient;
            _publishEndpoint = publishEndpoint;
            _db = db;
            _logger = logger;
        }

        public async Task<int> Handle(PlaceOrderCommand command)
        {
            _logger.LogInformation("🟢 Starting order processing for Amount: {Amount}", command.Amount);

            try
            {
                var order = new Order(command.Amount);

                // ✅ Save first
                _db.Orders.Add(order);
                await _db.SaveChangesAsync();

                _logger.LogInformation("💾 Order saved with ID: {OrderId}", order.Id);

                _logger.LogInformation("➡️ Calling Payment service for OrderId: {OrderId}", order.Id);

                var metadata = new Metadata();

                if (!string.IsNullOrEmpty(command.CorrelationId))
                {
                    metadata.Add("x-correlation-id", command.CorrelationId);
                }

                var payment = await _paymentClient.ProcessPaymentAsync(
                    new PaymentRequest
                    {
                        OrderId = order.Id,
                        Amount = order.Amount
                    },
                    metadata
                    );

                if (!payment.Success)
                {
                    _logger.LogError("❌ Payment failed for OrderId: {OrderId}", order.Id);
                    throw new Exception("Payment failed");
                }

                _logger.LogInformation("✅ Payment successful for OrderId: {OrderId}", order.Id);

                _logger.LogInformation("➡️ Publishing OrderPlaced event for OrderId: {OrderId}", order.Id);

                await _publishEndpoint.Publish(new OrderPlaced
                {
                    OrderId = order.Id,
                    Amount = order.Amount,
                    CorrelationId = command.CorrelationId
                });

                _logger.LogInformation("📩 OrderPlaced event published for OrderId: {OrderId}", order.Id);

                return order.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔥 Error occurred while processing order");

                throw; // rethrow so API returns proper error
            }
        }
    }
}