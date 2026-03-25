using Contract.Consumers;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// ✅ Add Controllers
builder.Services.AddControllers();

// ✅ Swagger (instead of AddOpenApi)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderPlacedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        // Queue for consuming messages
        cfg.ReceiveEndpoint("order-queue", e =>
        {
            e.ConfigureConsumer<OrderPlacedConsumer>(context);
        });
    });
});

var app = builder.Build();

// ✅ Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ✅ Map Controllers (IMPORTANT)
app.MapControllers();

app.Run();