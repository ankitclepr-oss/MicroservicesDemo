using PaymentService.Services;

var builder = WebApplication.CreateBuilder(args);

// ✅ Add gRPC
builder.Services.AddGrpc();

// ✅ Add Controllers
builder.Services.AddControllers();

// ✅ Swagger (optional but useful)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// ✅ Map gRPC service
app.MapGrpcService<PaymentGrpcService>();

// ✅ Map Controllers
app.MapControllers();

app.Run();