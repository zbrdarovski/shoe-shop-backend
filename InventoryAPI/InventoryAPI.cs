using InventoryAPI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "InventoryAPI", Version = "v1" });

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

var jwtSecret = builder.Configuration.GetValue<string>("JWT_SECRET");
var key = Encoding.ASCII.GetBytes("ProjektSUA ProjektSUA ProjektSUA");

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

var mongoDbConnectionString = builder.Configuration.GetValue<string>("MONGODB_CONNECTION_STRING");

builder.Services.AddSingleton<InventoryRepository>(serviceProvider =>
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
    return new InventoryRepository(mongoDbConnectionString);
});

builder.Services.AddLogging();

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

builder.Services.AddSingleton<RabbitMQService>();

var app = builder.Build();

app.UseMiddleware<ApiRequestMiddleware>();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "InventoryAPI");
});

app.UseRouting();
app.UseCors("AllowAllOrigins");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
