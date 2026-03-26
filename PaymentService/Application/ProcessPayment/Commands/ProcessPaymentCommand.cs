namespace PaymentService.Application.ProcessPayment.Commands
{
    public record ProcessPaymentCommand(int OrderId, double Amount);
}
