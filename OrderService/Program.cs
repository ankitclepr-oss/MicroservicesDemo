using Contract.Messages;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OrderService.Application.Common.Interfaces;
using OrderService.Application.Handler.GetOrders;
using OrderService.Application.Handler.PlaceOrder;
using OrderService.Infrastructure.Data;
using OrderService.Infrastructure.Resilience;
using Payment.Proto;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<GrpcResilienceInterceptor>();

// ✅ OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("OrderService"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddGrpcClientInstrumentation()
            .AddJaegerExporter(o =>
            {
                o.AgentHost = "localhost";
                o.AgentPort = 6831;
            });
    });

// ✅ Serilog
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration)
          .Enrich.WithProperty("Service", "OrderService");
});

// ✅ DB
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IOrderDbContext>(provider =>
    provider.GetRequiredService<OrderDbContext>());

// ✅ Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Handlers
builder.Services.AddScoped<PlaceOrderHandler>();
builder.Services.AddScoped<GetOrdersHandler>();

// ✅ RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.Message<OrderPlaced>(m =>
        {
            m.SetEntityName("order-queue");
        });
    });
});

// ✅ gRPC CLIENT + POLLY
builder.Services.AddGrpcClient<PaymentGrpc.PaymentGrpcClient>(o =>
{
    o.Address = new Uri("https://localhost:7002");
})
.AddInterceptor<GrpcResilienceInterceptor>(); 

var app = builder.Build();


// ✅ CORRELATION ID (FIXED)
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault();

    if (string.IsNullOrEmpty(correlationId))
    {
        correlationId = Guid.NewGuid().ToString(); // 🔥 generate if missing
    }

    context.Response.Headers["X-Correlation-Id"] = correlationId;

    using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
    {
        await next();
    }
});


// ✅ GLOBAL EXCEPTION HANDLER
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

    try
    {
        await next();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "🔥 Unhandled exception");

        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Internal Server Error"
        });
    }
});


// ✅ REQUEST LOGGING (CLEAN)
app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (httpContext, elapsed, ex) =>
    {
        if (ex != null || httpContext.Response.StatusCode >= 500)
            return LogEventLevel.Error;

        if (elapsed > 1000)
            return LogEventLevel.Warning;

        return LogEventLevel.Information;
    };
});


// ✅ Swagger
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
app.MapControllers();

app.Run();