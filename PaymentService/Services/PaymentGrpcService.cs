using Grpc.Core;
using Payment.Proto;
using PaymentService.Application.ProcessPayment.Commands;
using PaymentService.Application.ProcessPayment.Handlers;
using Serilog.Context;

namespace PaymentService.Services
{
    public class PaymentGrpcService : PaymentGrpc.PaymentGrpcBase
    {
        private readonly ProcessPaymentHandler _handler;
        private readonly ILogger<PaymentGrpcService> _logger;

        public PaymentGrpcService(
            ProcessPaymentHandler handler,
            ILogger<PaymentGrpcService> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        public override async Task<PaymentResponse> ProcessPayment(
            PaymentRequest request,
            ServerCallContext context)
        {
            // ✅ 1. Extract CorrelationId from gRPC headers
            var correlationId = context.RequestHeaders
                .FirstOrDefault(h => h.Key == "x-correlation-id")?.Value;

            // ✅ 2. Push into logging context
            using (LogContext.PushProperty("CorrelationId", correlationId ?? "N/A"))
            {
                _logger.LogInformation(
                    "💳 Processing payment for OrderId: {OrderId}, Amount: {Amount}",
                    request.OrderId,
                    request.Amount);

                try
                {
                    var command = new ProcessPaymentCommand(
                        request.OrderId,
                        request.Amount);

                    var result = await _handler.Handle(command);

                    _logger.LogInformation(
                        "✅ Payment processed for OrderId: {OrderId}",
                        request.OrderId);

                    return new PaymentResponse
                    {
                        Success = result.Success,
                        Message = result.Message
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "❌ Payment failed for OrderId: {OrderId}",
                        request.OrderId);

                    return new PaymentResponse
                    {
                        Success = false,
                        Message = "Payment failed"
                    };
                }
            }
        }
    }
}