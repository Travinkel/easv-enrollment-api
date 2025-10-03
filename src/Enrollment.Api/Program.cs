using Enrollment.Api;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<EnrollmentDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))); 


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