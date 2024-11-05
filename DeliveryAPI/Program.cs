using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DeliveryAPI;

var builder = WebApplication.CreateBuilder(args);

string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

// Add logging services
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    // You can add other logging providers here
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DeliveryAPI", Version = "v1" });

    // Define the Swagger security scheme for JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    // Define the Swagger security requirement for JWT
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    // Resolve conflicting actions
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});

var salt = builder.Configuration["Salt"];

// Add authentication and authorization services
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddSingleton<MongoDbContext>(sp =>
{
    string mongoDbConnectionString;

    if (environment == "Development")
    {
        // In Development, use the connection string from appsettings.json
        mongoDbConnectionString = builder.Configuration.GetConnectionString("MongoDBConnection");
    }
    else
    {
        // In non-Development, use the environment variable
        mongoDbConnectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") ?? "your_fallback_connection_string";
    }

    return new MongoDbContext(builder.Configuration);
});

builder.Services.AddAuthorization();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHealthChecks();

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

// RabbitMQ
var rabbitMQService = new RabbitMQService();
builder.Services.AddSingleton(rabbitMQService);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DeliveryAPI");
    c.RoutePrefix = "swagger"; // This sets the route prefix for Swagger UI
});

app.UseRouting();
app.UseCors("AllowAllOrigins");
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.MapPost("/api/deliveries", (DeliveryCreateDto deliveryDto, [FromServices] MongoDbContext dbContext, [FromServices] ILogger<Program> logger) =>
{
    rabbitMQService.SendLog(new LoggingEntry
    {
        Message = "Add delivery: " + deliveryDto,
        Timestamp = DateTime.UtcNow,
        Url = "/api/deliveries",
        CorrelationId = Guid.NewGuid().ToString(),
        ApplicationName = "DeliveryAPI",
        LogType = "Info"
    });
    try
    {
        var delivery = new Delivery
        {
            Id = deliveryDto.Id,
            UserId = deliveryDto.UserId,
            PaymentId = deliveryDto.PaymentId,
            Address = deliveryDto.Address,
            DeliveryTime = deliveryDto.DeliveryTime,
            GeoX = deliveryDto.GeoX,
            GeoY = deliveryDto.GeoY
        };

        dbContext.Deliveries.InsertOne(delivery);

        logger.LogInformation("Delivery added successfully: {DeliveryId}", delivery.Id);

        return Results.Ok(new { Message = "Delivery added successfully" });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error adding delivery");
        return Results.BadRequest(new { Message = "Error adding delivery" });
    }
}).WithName("AddDelivery").RequireAuthorization();

app.MapGet("/api/deliveries", ([FromServices] MongoDbContext dbContext, [FromServices] ILogger<Program> logger) =>
{
    rabbitMQService.SendLog(new LoggingEntry
    {
        Message = "Get deliveries",
        Timestamp = DateTime.UtcNow,
        Url = "/api/deliveries",
        CorrelationId = Guid.NewGuid().ToString(),
        ApplicationName = "DeliveryAPI",
        LogType = "Info"
    });
    try
    {
        var deliveries = dbContext.Deliveries.Find(_ => true).ToList();
        return Results.Ok(deliveries);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error retrieving deliveries");
        return Results.BadRequest(new { Message = "Error retrieving deliveries" });
    }
}).WithName("GetAllDeliveries");

app.MapGet("/api/deliveries/{id}", (string id, [FromServices] MongoDbContext dbContext, [FromServices] ILogger<Program> logger) =>
{
    try
    {
        rabbitMQService.SendLog(new LoggingEntry
        {
            Message = "Get delivery by ID: " + id,
            Timestamp = DateTime.UtcNow,
            Url = "/api/deliveries/{id}",
            CorrelationId = Guid.NewGuid().ToString(),
            ApplicationName = "DeliveryAPI",
            LogType = "Info"
        });
        var delivery = dbContext.Deliveries.Find(d => d.Id == id).FirstOrDefault();
        if (delivery == null)
        {
            return Results.NotFound(new { Message = "Delivery not found" });
        }

        return Results.Ok(delivery);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error retrieving delivery by ID");
        return Results.BadRequest(new { Message = "Error retrieving delivery by ID" });
    }
}).WithName("GetDeliveryById");

app.MapPut("/api/deliveries/{id}", (string id, DeliveryUpdateDto deliveryDto, [FromServices] MongoDbContext dbContext, [FromServices] ILogger<Program> logger) =>
{
    try
    {
        rabbitMQService.SendLog(new LoggingEntry
        {
            Message = "Update delivery with ID: " + id,
            Timestamp = DateTime.UtcNow,
            Url = "/api/deliveries/{id}",
            CorrelationId = Guid.NewGuid().ToString(),
            ApplicationName = "DeliveryAPI",
            LogType = "Info"
        });
        var delivery = dbContext.Deliveries.Find(d => d.Id == id).FirstOrDefault();

        if (delivery == null)
        {
            return Results.NotFound(new { Message = "Delivery not found" });
        }

        delivery.Address = deliveryDto.Address;
        delivery.GeoX = deliveryDto.GeoX;
        delivery.GeoY = deliveryDto.GeoY;

        dbContext.Deliveries.ReplaceOne(d => d.Id == id, delivery);

        logger.LogInformation("Delivery updated successfully: {DeliveryId}", delivery.Id);

        return Results.Ok(new { Message = "Delivery updated successfully" });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error updating delivery");
        return Results.BadRequest(new { Message = "Error updating delivery" });
    }
}).WithName("UpdateDelivery").RequireAuthorization();

app.MapDelete("/api/deliveries/{id}", (string id, [FromServices] MongoDbContext dbContext, [FromServices] ILogger<Program> logger) =>
{
    rabbitMQService.SendLog(new LoggingEntry
    {
        Message = "Delete delivery with ID: " + id,
        Timestamp = DateTime.UtcNow,
        Url = "/api/deliveries/{id}",
        CorrelationId = Guid.NewGuid().ToString(),
        ApplicationName = "DeliveryAPI",
        LogType = "Info"
    });
    try
    {
        var delivery = dbContext.Deliveries.Find(d => d.Id == id).FirstOrDefault();
        if (delivery == null)
        {
            return Results.NotFound(new { Message = "Delivery not found" });
        }

        dbContext.Deliveries.DeleteOne(d => d.Id == id);

        logger.LogInformation("Delivery deleted successfully: {DeliveryId}", delivery.Id);

        return Results.Ok(new { Message = "Delivery deleted successfully" });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error deleting delivery");
        return Results.BadRequest(new { Message = "Error deleting delivery" });
    }
}).WithName("DeleteDelivery").RequireAuthorization();

app.MapPut("/api/deliveries/update-coordinates/{id}", (string id, CoordinatesUpdateDto coordinatesDto, [FromServices] MongoDbContext dbContext, [FromServices] ILogger<Program> logger) =>
{
    rabbitMQService.SendLog(new LoggingEntry
    {
        Message = "Update delivery coordinates: " + coordinatesDto,
        Timestamp = DateTime.UtcNow,
        Url = "/api/deliveries/update-coordinates/{id}",
        CorrelationId = Guid.NewGuid().ToString(),
        ApplicationName = "DeliveryAPI",
        LogType = "Info"
    });
    try
    {
        var delivery = dbContext.Deliveries.Find(d => d.Id == id).FirstOrDefault();

        if (delivery == null)
        {
            return Results.NotFound(new { Message = "Delivery not found" });
        }

        delivery.GeoX = coordinatesDto.GeoX;
        delivery.GeoY = coordinatesDto.GeoY;

        dbContext.Deliveries.ReplaceOne(d => d.Id == id, delivery);

        logger.LogInformation("Coordinates updated successfully for delivery: {DeliveryId}", delivery.Id);

        return Results.Ok(new { Message = "Coordinates updated successfully" });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error updating coordinates");
        return Results.BadRequest(new { Message = "Error updating coordinates" });
    }
}).WithName("UpdateCoordinates").RequireAuthorization();

app.MapPut("/api/deliveries/update-delivery-time/{id}", (string id, DeliveryTimeUpdateDto deliveryTimeDto, [FromServices] MongoDbContext dbContext, [FromServices] ILogger<Program> logger) =>
{
    try
    {
        rabbitMQService.SendLog(new LoggingEntry
        {
            Message = "Updating delivery time for ID: " + id,
            Timestamp = DateTime.UtcNow,
            Url = "/api/deliveries/update-delivery-time/{id}",
            CorrelationId = Guid.NewGuid().ToString(),
            ApplicationName = "DeliveryAPI",
            LogType = "Info"
        });
        var delivery = dbContext.Deliveries.Find(d => d.Id == id).FirstOrDefault();

        if (delivery == null)
        {
            return Results.NotFound(new { Message = "Delivery not found" });
        }

        delivery.DeliveryTime = deliveryTimeDto.DeliveryTime;

        dbContext.Deliveries.ReplaceOne(d => d.Id == id, delivery);

        logger.LogInformation("Delivery time updated successfully for delivery: {DeliveryId}", delivery.Id);

        return Results.Ok(new { Message = "Delivery time updated successfully" });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error updating delivery time");
        return Results.BadRequest(new { Message = "Error updating delivery time" });
    }
}).WithName("UpdateDeliveryTime").RequireAuthorization();

app.MapPut("/api/deliveries/update-address/{id}", (string id, UpdateAddressDto addressDto, [FromServices] MongoDbContext dbContext, [FromServices] ILogger<Program> logger) =>
{
    try
    {
        rabbitMQService.SendLog(new LoggingEntry
        {
            Message = "Update delivery with ID: " + id,
            Timestamp = DateTime.UtcNow,
            Url = "/api/deliveries/update-address/{id}",
            CorrelationId = Guid.NewGuid().ToString(),
            ApplicationName = "DeliveryAPI",
            LogType = "Info"
        });
        var delivery = dbContext.Deliveries.Find(d => d.Id == id).FirstOrDefault();

        if (delivery == null)
        {
            return Results.NotFound(new { Message = "Delivery not found" });
        }

        delivery.Address = addressDto.Address;

        dbContext.Deliveries.ReplaceOne(d => d.Id == id, delivery);

        logger.LogInformation("Address updated successfully for delivery: {DeliveryId}", delivery.Id);

        return Results.Ok(new { Message = "Address updated successfully" });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error updating address");
        return Results.BadRequest(new { Message = "Error updating address" });
    }
}).WithName("UpdateAddress").RequireAuthorization();

app.UseHealthChecks("/api/deliveries/health");

app.Run();