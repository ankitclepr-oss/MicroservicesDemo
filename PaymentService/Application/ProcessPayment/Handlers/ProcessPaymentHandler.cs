using Microsoft.Extensions.Logging;
using PaymentService.Application.Common.Interfaces;
using PaymentService.Application.ProcessPayment.Commands;

namespace PaymentService.Application.ProcessPayment.Handlers
{
    public class ProcessPaymentHandler
    {
        private readonly IPaymentDbContext _db;
        private readonly ILogger<ProcessPaymentHandler> _logger;

        public ProcessPaymentHandler(
            IPaymentDbContext db,
            ILogger<ProcessPaymentHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<(bool Success, string Message)> Handle(ProcessPaymentCommand command)
        {
            _logger.LogInformation("💳 Starting payment processing for OrderId: {OrderId}, Amount: {Amount}",
                command.OrderId, command.Amount);

            try
            {
                var payment = new PaymentService.Domain.Entities.Payment(command.OrderId, command.Amount);

                // ✅ Save to DB
                _db.Payments.Add(payment);
                await _db.SaveChangesAsync();

                _logger.LogInformation("💾 Payment saved successfully with PaymentId: {PaymentId} for OrderId: {OrderId}",
                    payment.Id, command.OrderId);

                return (true, "Payment successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Payment failed for OrderId: {OrderId}, Amount: {Amount}",
                    command.OrderId, command.Amount);

                return (false, "Payment failed");
            }
        }
    }
}