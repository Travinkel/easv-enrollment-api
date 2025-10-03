using Enrollment.Api;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? builder.Configuration["DATABASE_URL"];

builder.Services.AddDbContext<EnrollmentDbContext>(opt =>
    opt.UseNpgsql(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
    
// CORS policy for web clients
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWeb", policy =>
    {
        policy.WithOrigins("http://localhost:5173",
                "https://travinkel.github.io"
            )
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EnrollmentDbContext>();
    db.Database.Migrate();
}

// Enable CORS before routes
app.UseCors("AllowWeb");

// Root health check
app.MapGet("/", () => Results.Ok("EASV Enrollment API (Unofficial) is running ✅"));

// Configure the HTTP request pipeline.
app.MapGet("/enrollment", async (EnrollmentDbContext db) => await db.Enrollments.ToListAsync());

// Hook up enrollment endpoints
app.MapEnrollmentEndpoints();

app.Run();

public partial class Program { }