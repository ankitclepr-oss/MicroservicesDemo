using Contract.Messages;
using MassTransit;

namespace Contract.Consumers;

public class OrderPlacedConsumer : IConsumer<OrderPlaced>
{
    public Task Consume(ConsumeContext<OrderPlaced> context)
    {
        Console.WriteLine("rabbitMQ - Consuming Order Placed Event");

        var msg = context.Message;

        Console.WriteLine($"📧 Email sent for Order {msg.OrderId}, Amount: {msg.Amount}");

        return Task.CompletedTask;
    }
}