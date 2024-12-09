using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using StatsBrdarovski;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "StatsBrdarovski API", Version = "v1" });
});

// Configure MongoDB services
string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    string mongoDbConnectionString;

    if (environment == "Development")
    {
        mongoDbConnectionString = builder.Configuration.GetConnectionString("MongoDBConnection");
    }
    else
    {
        mongoDbConnectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING")
                                  ?? "mongodb_connection_string";
    }

    return new MongoClient(mongoDbConnectionString);
});

builder.Services.AddSingleton(s =>
{
    var client = s.GetRequiredService<IMongoClient>();
    return client.GetDatabase("StatsDatabase");
});

builder.Services.AddScoped<StatsRepository>();

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin() // Allow all origins
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "StatsBrdarovski API v1");
    });
}

app.UseRouting();
app.UseCors("AllowAllOrigins");
app.UseAuthorization();

app.MapControllers();

app.Run();
