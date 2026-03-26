using System.Text.Json.Serialization;

namespace OrderService.Application.Commands.PlaceOrder
{
    public record PlaceOrderCommand(
        double Amount,
        [property: JsonIgnore]
        string? CorrelationId);
}
