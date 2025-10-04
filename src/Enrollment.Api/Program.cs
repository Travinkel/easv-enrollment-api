using Enrollment.Api;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? builder.Configuration["DATABASE_URL"];

builder.Services.AddDbContext<EnrollmentDbContext>(opt =>
    opt.UseNpgsql(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EASV Enrollment API",
        Version = "v1",
        Description = "Minimal API slice demonstrating enrollment management (create, confirm, cancel).",
        Contact = new OpenApiContact
        {
            Name = "Stefan Ankersø",
            Url = new Uri("https://github.com/Travinkel/easv-enrollment-api")
        }
    });
});

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
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "EASV Enrollment API v1"); });

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EnrollmentDbContext>();
    db.Database.Migrate();
    
    // Seed demo data if none exists
    if (!db.Enrollments.Any())
    {
        db.Enrollments.AddRange(
            new Enrollment.Api.Enrollment { StudentId = Guid.NewGuid(), CourseId = Guid.NewGuid(), Status = "Pending" },
            new Enrollment.Api.Enrollment { StudentId = Guid.NewGuid(), CourseId = Guid.NewGuid(), Status = "Confirmed" },
            new Enrollment.Api.Enrollment { StudentId = Guid.NewGuid(), CourseId = Guid.NewGuid(), Status = "Cancelled" }
        );
        db.SaveChanges();
    }
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

public partial class Program
{
}