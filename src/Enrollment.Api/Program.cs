using Enrollment.Api;
using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);

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

// Hook up enrollment endpoints
app.MapEnrollmentEndpoints();

app.Run();