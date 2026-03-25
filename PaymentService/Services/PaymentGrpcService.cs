using Grpc.Core;
using Payment.Proto;

namespace PaymentService.Services
{
    public class PaymentGrpcService : PaymentGrpc.PaymentGrpcBase
    {
        public override Task<PaymentResponse> ProcessPayment(PaymentRequest request, ServerCallContext context)
        {
            Console.WriteLine($"gRPC - Payment received for Order {request.OrderId}");

            return Task.FromResult(new PaymentResponse
            {
                Success = true,
                Message = "Payment Successful"
            });
        }
    }
}
