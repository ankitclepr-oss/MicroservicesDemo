using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OrderService.Application.Handler.GetOrders;
using PaymentService.Application.Common.Interfaces;
using PaymentService.Application.ProcessPayment.Handlers;
using PaymentService.Infrastructure.Data;
using PaymentService.Services;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("PaymentService")) // change per service

            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddGrpcClientInstrumentation()

            .AddJaegerExporter(o =>
            {
                o.AgentHost = "localhost";
                o.AgentPort = 6831;
            });
    });

builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration).Enrich.WithProperty("Service", "PaymentService");
});

builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Interface mapping
builder.Services.AddScoped<IPaymentDbContext>(provider =>
    provider.GetRequiredService<PaymentDbContext>());

// ✅ Add gRPC
builder.Services.AddGrpc();

// ✅ Add Controllers
builder.Services.AddControllers();

// ✅ Swagger (optional but useful)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ProcessPaymentHandler>();
builder.Services.AddScoped<GetPaymentsHandler>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault();

    if (!string.IsNullOrEmpty(correlationId))
    {
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next();
        }
    }
    else
    {
        await next();
    }
});

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

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

app.UseHttpsRedirection();

// ✅ Map gRPC service
app.MapGrpcService<PaymentGrpcService>();

// ✅ Map Controllers
app.MapControllers();

app.Run();