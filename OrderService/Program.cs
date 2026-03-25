using Contract.Messages;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.Common.Interfaces;
using OrderService.Application.Handler.PlaceOrder;
using OrderService.Infrastructure.Data;
using Payment.Proto;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IOrderDbContext>(provider =>
    provider.GetRequiredService<OrderDbContext>());

// Add services to the container
builder.Services.AddControllers();

// ✅ Swagger (optional but useful)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Register Handler
builder.Services.AddScoped<PlaceOrderHandler>();

// RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.Message<Contract.Messages.OrderPlaced>(m =>
        {
            m.SetEntityName("order-queue");
        });
    });
});

// gRPC Client
builder.Services.AddGrpcClient<PaymentGrpc.PaymentGrpcClient>(o =>
{
    o.Address = new Uri("https://localhost:7282"); // match PaymentService
});

var app = builder.Build();

// ✅ Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

// Map controllers
app.MapControllers();

app.Run();