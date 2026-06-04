using Tcc.Api.Interfaces;
using Tcc.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKeyedScoped<ITelemetryProcessor, ClassicTelemetryService>("classic");
builder.Services.AddKeyedScoped<ITelemetryProcessor, OptimizedTelemetryService>("optimized");

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

