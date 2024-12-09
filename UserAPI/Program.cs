using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserAPI;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using System;

var builder = WebApplication.CreateBuilder(args);

string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

builder.Services.AddHttpClient();

// Add logging services
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    // You can add other logging providers here
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "UserAPI", Version = "v1" });

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
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserAPI");
    c.RoutePrefix = "swagger"; // This sets the route prefix for Swagger UI
});

app.UseRouting();
app.UseCors("AllowAllOrigins");
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapPost("/api/users/register", async (UserRegistrationDto userDto, [FromServices] MongoDbContext dbContext, [FromServices] ILogger<Program> logger, [FromServices] IHttpClientFactory httpClientFactory, [FromServices] IConfiguration configuration, [FromServices] IHttpContextAccessor httpContextAccessor) =>
{
    rabbitMQService.SendLog(new LoggingEntry
    {
        Message = "Register user: " + userDto,
        Timestamp = DateTime.UtcNow,
        Url = "/api/users/register",
        CorrelationId = Guid.NewGuid().ToString(),
        ApplicationName = "UserAPI",
        LogType = "Info"
    });

    try
    {
        // Combine the password and salt, then hash the result
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDto.Password + salt);

        var user = new User { Id = userDto.Id, Username = userDto.Username, Password = hashedPassword };
        dbContext.Users.InsertOne(user);

        logger.LogInformation("User registered successfully: {UserId}", user.Id);

        // Generate JWT token
        var token = GenerateJwtToken(user.Id, configuration["Jwt:Key"], configuration["Jwt:Issuer"], httpContextAccessor.HttpContext);

        // Create a new cart for the user by calling the cart creation API
        var cartCreationUrl = $"https://localhost:11183/CartPayment/createcart/{user.Id}";

        using (var httpClient = httpClientFactory.CreateClient())
        {
            // Add the Bearer token to the request
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.PostAsync(cartCreationUrl, null);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Cart created successfully for user: {UserId}", user.Id);
                return Results.Ok(new { Message = "Registration successful", Token = token });
            }
            else
            {
                logger.LogError("Error creating cart for user: {UserId}", user.Id);
                return Results.BadRequest(new { Message = "Error creating cart for user" });
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error registering user");
        return Results.BadRequest(new { Message = "Error registering user" });
    }
}).WithName("Register");


string GenerateJwtToken(string userId, string key, string issuer, HttpContext httpContext)
{
    if (userId is null || key is null || issuer is null)
    {
        throw new ArgumentNullException("userId, key, and issuer cannot be null.");
    }

    var tokenHandler = new JwtSecurityTokenHandler();
    var tokenKey = Encoding.ASCII.GetBytes(key);

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, userId)
        }),
        Expires = DateTime.UtcNow.AddHours(1),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature),
        Issuer = issuer
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    var tokenString = tokenHandler.WriteToken(token);

    return tokenString;
}

app.MapPost("/api/users/login", (UserLoginDto userDto, [FromServices] MongoDbContext dbContext, [FromServices] IConfiguration configuration, [FromServices] IHttpContextAccessor httpContextAccessor, [FromServices] ILogger<Program> logger) =>
{
    rabbitMQService.SendLog(new LoggingEntry
    {
        Message = "Login user: " + userDto,
        Timestamp = DateTime.UtcNow,
        Url = "/api/users/login",
        CorrelationId = Guid.NewGuid().ToString(),
        ApplicationName = "UserAPI",
        LogType = "Info"
    });
    var user = dbContext.Users.Find(u => u.Username == userDto.Username).FirstOrDefault();

    if (user == null || !BCrypt.Net.BCrypt.Verify(userDto.Password + salt, user.Password))
    {
        logger.LogWarning("Invalid login attempt for user: {Username}", userDto.Username);
        return Results.Unauthorized();
    }

    // Generate JWT token
    var token = GenerateJwtToken(user.Id, configuration["Jwt:Key"], configuration["Jwt:Issuer"], httpContextAccessor.HttpContext);

    // Return token and user ID in response
    return Results.Ok(new { Message = "Connection successful", Token = token, UserId = user.Id });
}).WithName("Login");


app.MapPost("/api/users/change-password", (ChangePasswordDto changePasswordDto, [FromServices] MongoDbContext dbContext, [FromServices] IHttpContextAccessor httpContextAccessor, [FromServices] ILogger<Program> logger) =>
{
    rabbitMQService.SendLog(new LoggingEntry
    {
        Message = "Change password: " + changePasswordDto,
        Timestamp = DateTime.UtcNow,
        Url = "/api/users/change-password",
        CorrelationId = Guid.NewGuid().ToString(),
        ApplicationName = "UserAPI",
        LogType = "Info"
    });
    var user = dbContext.Users.Find(u => u.Username == changePasswordDto.Username).FirstOrDefault();

    if (user == null || !BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword + salt, user.Password))
    {
        logger.LogWarning("Invalid password change attempt for user: {Username}", changePasswordDto.Username);
        return Results.Unauthorized();
    }

    // Combine the new password and salt, then hash the result
    string hashedNewPassword = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword + salt);

    user.Password = hashedNewPassword;

    dbContext.Users.ReplaceOne(u => u.Id == user.Id, user);

    logger.LogInformation("Password changed successfully for user: {Username}", user.Username);

    return Results.Ok(new { Message = "Password changed successfully" });
}).WithName("ChangePassword").RequireAuthorization();

app.MapGet("/api/users", ([FromServices] MongoDbContext dbContext, [FromServices] ILogger<Program> logger) =>
{
    rabbitMQService.SendLog(new LoggingEntry
    {
        Message = "Get users",
        Timestamp = DateTime.UtcNow,
        Url = "/api/users",
        CorrelationId = Guid.NewGuid().ToString(),
        ApplicationName = "UserAPI",
        LogType = "Info"
    });
    try
    {
        var users = dbContext.Users.Find(_ => true).ToList();
        return Results.Ok(users);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error retrieving users");
        return Results.BadRequest(new { Message = "Error retrieving users" });
    }
}).WithName("GetAllUsers");

app.MapGet("/api/users/{id}", (string id, [FromServices] MongoDbContext dbContext, [FromServices] ILogger<Program> logger) =>
{
    rabbitMQService.SendLog(new LoggingEntry
    {
        Message = "Get user",
        Timestamp = DateTime.UtcNow,
        Url = "/api/users/{id}",
        CorrelationId = Guid.NewGuid().ToString(),
        ApplicationName = "UserAPI",
        LogType = "Info"
    });
    try
    {
        var user = dbContext.Users.Find(u => u.Id == id).FirstOrDefault();
        if (user == null)
        {
            return Results.NotFound(new { Message = "User not found" });
        }

        return Results.Ok(user);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error retrieving user by ID");
        return Results.BadRequest(new { Message = "Error retrieving user by ID" });
    }
}).WithName("GetUserById");

app.MapPut("/api/users/{id}", (string id, UserUpdateDto userDto, [FromServices] MongoDbContext dbContext, [FromServices] ILogger<Program> logger) =>
{
    rabbitMQService.SendLog(new LoggingEntry
    {
        Message = "Update user: " + userDto,
        Timestamp = DateTime.UtcNow,
        Url = "/api/users/{id}",
        CorrelationId = Guid.NewGuid().ToString(),
        ApplicationName = "UserAPI",
        LogType = "Info"
    });
    try
    {
        var user = dbContext.Users.Find(u => u.Id == id).FirstOrDefault();
        if (user == null)
        {
            return Results.NotFound(new { Message = "User not found" });
        }

        user.Username = userDto.Username;
        dbContext.Users.ReplaceOne(u => u.Id == id, user);

        logger.LogInformation("User updated successfully: {UserId}", user.Id);

        return Results.Ok(new { Message = "User updated successfully" });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error updating user");
        return Results.BadRequest(new { Message = "Error updating user" });
    }
}).WithName("UpdateUser").RequireAuthorization();

app.MapDelete("/api/users/{id}", (string id, [FromServices] MongoDbContext dbContext, [FromServices] ILogger<Program> logger) =>
{
    rabbitMQService.SendLog(new LoggingEntry
    {
        Message = "Delete user: " + id,
        Timestamp = DateTime.UtcNow,
        Url = "/api/users/{id}",
        CorrelationId = Guid.NewGuid().ToString(),
        ApplicationName = "UserAPI",
        LogType = "Info"
    });
    try
    {
        var user = dbContext.Users.Find(u => u.Id == id).FirstOrDefault();
        if (user == null)
        {
            return Results.NotFound(new { Message = "User not found" });
        }

        dbContext.Users.DeleteOne(u => u.Id == id);

        logger.LogInformation("User deleted successfully: {UserId}", user.Id);

        return Results.Ok(new { Message = "User deleted successfully" });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error deleting user");
        return Results.BadRequest(new { Message = "Error deleting user" });
    }
}).WithName("DeleteUser").RequireAuthorization();

app.MapGet("/api/users/username/{username}", (string username, [FromServices] MongoDbContext dbContext, [FromServices] ILogger<Program> logger) =>
{
    rabbitMQService.SendLog(new LoggingEntry
    {
        Message = "Get user by username: " + username,
        Timestamp = DateTime.UtcNow,
        Url = "/api/users/username/{id}",
        CorrelationId = Guid.NewGuid().ToString(),
        ApplicationName = "UserAPI",
        LogType = "Info"
    });
    try
    {
        var user = dbContext.Users.Find(u => u.Username == username).FirstOrDefault();
        if (user == null)
        {
            return Results.NotFound(new { Message = "User not found" });
        }

        return Results.Ok(user);
    }
    catch (Exception ex)
    {
        rabbitMQService.SendLog(new LoggingEntry
        {
            Message = "Error retrieving user by username: " + ex.Message,
            Timestamp = DateTime.UtcNow,
            Url = "/api/users/username/{id}",
            CorrelationId = Guid.NewGuid().ToString(),
            ApplicationName = "UserAPI",
            LogType = "Error"
        });
        logger.LogError(ex, "Error retrieving user by username");
        return Results.BadRequest(new { Message = "Error retrieving user by username" });
    }
}).WithName("GetUserByUsername");

app.UseHealthChecks("/api/users/health");

app.Run();