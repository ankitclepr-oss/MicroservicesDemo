using EmailService.Application.Common.Interfaces;
using EmailService.Domain.Entities;

namespace EmailService.Events.OrderPlaced
{
    public class OrderPlacedHandler
    {
        private readonly IEmailService _emailService;

        public OrderPlacedHandler(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task Handle(Contract.Messages.OrderPlaced message)
        {
            Console.WriteLine($"📩 Handling OrderPlaced event for Order {message.OrderId}");

            var email = new Email(
                "customer@test.com",
                "Order Confirmation",
                $"Your order {message.OrderId} of amount {message.Amount} is confirmed.");

            await _emailService.SendAsync(email);
        }
    }
}
