using EmailService.Application.Common.Interfaces;
using EmailService.Consumers;
using EmailService.Events.OrderPlaced;
using MassTransit;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .WithTracing(b =>
    {
        b.SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddService("EmailService"))
         .AddSource("MassTransit")
         .AddJaegerExporter(o =>
         {
             o.AgentHost = "localhost";
             o.AgentPort = 6831;
         });
    });

builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration)
    .Enrich.WithProperty("Service", "EmailService");
});

// ✅ Add Controllers
builder.Services.AddControllers();

// ✅ Swagger (instead of AddOpenApi)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Register Email Service
builder.Services.AddScoped<IEmailService, EmailService.Infrastructure.Services.EmailService>();

// ✅ Register Handler
builder.Services.AddScoped<OrderPlacedHandler>();

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

// 🔥 GLOBAL EXCEPTION LOGGING (ADD HERE)
app.Use(async (context, next) =>
{
    var logger = context.RequestServices
        .GetRequiredService<ILogger<Program>>();

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

// ✅ Request logging
app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (httpContext, elapsed, ex) =>
    {
        // Only log slow or failed requests
        if (ex != null || httpContext.Response.StatusCode >= 500)
            return LogEventLevel.Error;

        if (elapsed > 1000) // slow request
            return LogEventLevel.Warning;

        return LogEventLevel.Information;
    };
});

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