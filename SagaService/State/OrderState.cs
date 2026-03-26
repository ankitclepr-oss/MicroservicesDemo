using MassTransit;

namespace SagaService.State
{
    public class OrderState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }

        public int OrderId { get; set; }

        public string CurrentState { get; set; } = default!;
    }
}