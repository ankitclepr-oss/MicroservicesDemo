using MassTransit;
using SagaService.State;
using SagaService.StateMachine;

var builder = WebApplication.CreateBuilder(args);

// MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddSagaStateMachine<OrderStateMachine, OrderState>()
        .InMemoryRepository(); // later → SQL

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.MapGet("/", () => "Saga Service Running");

app.Run();