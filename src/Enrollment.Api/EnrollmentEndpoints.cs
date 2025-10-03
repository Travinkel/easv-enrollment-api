using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Enrollment.Api;

public static class EnrollmentEndpoints
{
    public static IEndpointRouteBuilder MapEnrollmentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/enrollments", async (EnrollmentRequest request, EnrollmentDbContext db) =>
        {
            if (request.StudentId == Guid.Empty || request.CourseId == Guid.Empty)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["Ids"] = new[] { "StudentId and CourseId must be valid GUIDs." }
                });
            }

            var enrollment = new Enrollment
            {
                Id = Guid.NewGuid(),
                StudentId = request.StudentId,
                CourseId = request.CourseId,
                Status = "Pending"
            };

            try
            {
                db.Enrollments.Add(enrollment);
                await db.SaveChangesAsync();
                return Results.Created($"/enrollments/{enrollment.Id}", enrollment);
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate") == true)
            {
                return Results.Conflict(new { Message = "Student is already enrolled in this course." });
            }
        });

        app.MapGet("/enrollments/{id:guid}", async (Guid id, EnrollmentDbContext db) =>
        {
            var enrollment = await db.Enrollments.FindAsync(id);
            return enrollment is null
                ? Results.NotFound(new { Message = $"Enrollment {id} not found." })
                : Results.Ok(enrollment);
        });
        app.MapPost("/enrollments/{id:guid}/confirm", async (Guid id, EnrollmentDbContext db) =>
        {
            var enrollment = await db.Enrollments.FindAsync(id);
            if (enrollment is null) return Results.NotFound();

            if (enrollment.Status != "Pending")
                return Results.Conflict(new { Message = "Enrollment not in Pending state." });

            enrollment.Status = "Confirmed";
            
            try
            {
                await db.SaveChangesAsync();
                return Results.Ok(enrollment);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Rare case: Two users try to confirm at once
                return Results.Conflict(new { Message = "Concurrent update detected. Try Again." });
            }
        });

        // Cancel enrollment
        app.MapPost("/enrollments/{id:guid}/cancel", async (Guid id, EnrollmentDbContext db) =>
        {
            var enrollment = await db.Enrollments.FindAsync(id);
            if (enrollment is null) return Results.NotFound();

            if (enrollment.Status == "Completed" || enrollment.Status == "Cancelled")
                return Results.Conflict(new { Message = "Enrollment already completed or cancelled." });

            enrollment.Status = "Cancelled";
            
            try
            {
                await db.SaveChangesAsync();
                return Results.Ok(enrollment);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Results.Conflict(new { Message = "Concurrent update detected. Try again." });
            }
        });

        return app;
    }
}

public record EnrollmentRequest(Guid StudentId, Guid CourseId);