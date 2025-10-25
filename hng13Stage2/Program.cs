
using System.Threading.Channels;
using hng13Stage2.Data;
using hng13Stage2.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
DotNetEnv.Env.TraversePath().Load();

var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") 
    ?? throw new InvalidOperationException("CONNECTION_STRING environment variable is not set.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<CountryService>();
builder.Services.AddScoped<ExternalApiService>();

// Image Service
builder.Services.AddSingleton<ImageService>();
builder.Services.AddSingleton(Channel.CreateBounded<ImageDto>(50));
builder.Services.AddHostedService<ImageBackgroundWorker>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAny", builder =>
    {
        builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyOrigin();
    });
});
builder.Services.AddMemoryCache();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/", () => "HNGi 13 Stage 2 API is running...");

app.UseHttpsRedirection();
app.UseCors("AllowAny");
app.UseAuthorization();

app.MapControllers();

app.Run();
