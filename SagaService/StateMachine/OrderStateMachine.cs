using MassTransit;
using SagaService.State;
using Contract.Messages;

namespace SagaService.StateMachine
{
    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public MassTransit.State AwaitingPayment { get; private set; }
        public MassTransit.State Completed { get; private set; }
        public MassTransit.State Failed { get; private set; }

        public Event<OrderCreated> OrderCreatedEvent { get; private set; }
        public Event<PaymentCompleted> PaymentCompletedEvent { get; private set; }
        public Event<PaymentFailed> PaymentFailedEvent { get; private set; }

        public OrderStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Event(() => OrderCreatedEvent,
                x => x.CorrelateById(m => m.Message.CorrelationId));

            Event(() => PaymentCompletedEvent,
                x => x.CorrelateById(m => m.Message.CorrelationId));

            Event(() => PaymentFailedEvent,
                x => x.CorrelateById(m => m.Message.CorrelationId));

            Initially(
                When(OrderCreatedEvent)
                    .Then(ctx =>
                    {
                        ctx.Saga.OrderId = ctx.Message.OrderId;
                    })
                    .TransitionTo(AwaitingPayment)
                    .Publish(ctx => new ProcessPayment
                    {
                        CorrelationId = ctx.Message.CorrelationId,
                        OrderId = ctx.Message.OrderId,
                        Amount = ctx.Message.Amount
                    })
            );

            During(AwaitingPayment,
                When(PaymentCompletedEvent)
                    .TransitionTo(Completed)
                    .Publish(ctx => new OrderCompleted
                    {
                        OrderId = ctx.Saga.OrderId
                    }),

                When(PaymentFailedEvent)
                    .TransitionTo(Failed)
                    .Publish(ctx => new OrderFailed
                    {
                        OrderId = ctx.Saga.OrderId
                    })
            );
        }
    }
}