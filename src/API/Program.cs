using API.Endpoints;
using API.Middleware;
using Application.Services;
using Domain.Repositories;
using Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Register repositories
builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();

// Register application services
builder.Services.AddScoped<ProductApplicationService>();

var app = builder.Build();

app.UseExceptionHandler();

app.MapProductEndpoints();

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
