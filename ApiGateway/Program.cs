using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Context;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("ApiGateway")) // change per service

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
    .Enrich.WithProperty("Service", "ApiGateway");
});

// ✅ Load ocelot.json
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// ✅ Add Ocelot
builder.Services.AddOcelot();

var app = builder.Build();

app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                        ?? Guid.NewGuid().ToString();

    context.Response.Headers["X-Correlation-Id"] = correlationId;

    using (LogContext.PushProperty("CorrelationId", correlationId))
    {
        await next();
    }
});

// 🔥 GLOBAL EXCEPTION HANDLER (only for non-Ocelot endpoints like "/")
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

// ✅ Root endpoint (prevents 404 warning)
app.MapGet("/", () => "Ocelot Gateway Running");

// ⚠️ IMPORTANT: Ocelot should be LAST
await app.UseOcelot();

app.Run();