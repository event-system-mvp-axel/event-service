using EventService.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using static System.Net.Mime.MediaTypeNames;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Event Service API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

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
});

// Add Database
builder.Services.AddDbContext<EventContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true
    };
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Azure App Service port binding
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Create database and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EventContext>();

    try
    {
        // Förstör och återskapa databasen för att säkerställa att den är uppdaterad
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // Lägg till events direkt
        var events = new[]
        {
            new EventService.Models.Event
            {
                Id = Guid.NewGuid(),
                Title = "Sommarkonsert i Parken",
                Description = "En fantastisk utomhuskonsert med lokala artister",
                Location = "Stadsparken, Stockholm",
                StartDate = DateTime.UtcNow.AddDays(22).Date.AddHours(14),
                EndDate = DateTime.UtcNow.AddDays(22).Date.AddHours(22),
                Category = "Musik",
                MaxTickets = 500,
                Price = 299,
                ImageUrl = "/images/sommarkonsert.png",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new EventService.Models.Event
            {
                Id = Guid.NewGuid(),
                Title = "Matfestival 2025",
                Description = "Smaka på delikatesser från hela världen",
                Location = "Kungsträdgården, Stockholm",
                StartDate = DateTime.UtcNow.AddDays(30).Date.AddHours(11),
                EndDate = DateTime.UtcNow.AddDays(30).Date.AddHours(20),
                Category = "Mat & Dryck",
                MaxTickets = 1000,
                Price = 150,
                ImageUrl = "/images/matdagar.png",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new EventService.Models.Event
            {
                Id = Guid.NewGuid(),
                Title = "Stand-up Comedy Night",
                Description = "En kväll fylld med skratt och underhållning",
                Location = "Norra Brunn, Stockholm",
                StartDate = DateTime.UtcNow.AddDays(14).Date.AddHours(20).AddMinutes(30),
                EndDate = DateTime.UtcNow.AddDays(14).Date.AddHours(23).AddMinutes(30),
                Category = "Komedi",
                MaxTickets = 200,
                Price = 350,
                ImageUrl = "/images/standup.png",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.Events.AddRange(events);
        context.SaveChanges();
        Console.WriteLine("Database seeded successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Seeding error: {ex.Message}");
    }
}

app.MapGet("/health", () => "Service is running!");
app.MapGet("/", () => "Service is healthy");

app.Run();