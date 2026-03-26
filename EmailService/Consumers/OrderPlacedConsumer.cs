using Contract.Messages;
using EmailService.Events.OrderPlaced;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EmailService.Consumers;

public class OrderPlacedConsumer : IConsumer<OrderPlaced>
{
    private readonly OrderPlacedHandler _handler;
    private readonly ILogger<OrderPlacedConsumer> _logger;

    public OrderPlacedConsumer(
        OrderPlacedHandler handler,
        ILogger<OrderPlacedConsumer> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var message = context.Message;

        _logger.LogInformation("📥 Received OrderPlaced event for OrderId: {OrderId}, Amount: {Amount}",
            message.OrderId, message.Amount);

        try
        {
            using (Serilog.Context.LogContext.PushProperty("CorrelationId", context.Message.CorrelationId))
            {
                await _handler.Handle(context.Message);
            }

            _logger.LogInformation("✅ Successfully processed OrderPlaced event for OrderId: {OrderId}",
                message.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "❌ Error processing OrderPlaced event for OrderId: {OrderId}",
                message.OrderId);

            throw; // IMPORTANT: let MassTransit handle retries
        }
    }
}