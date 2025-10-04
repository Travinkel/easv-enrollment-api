using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Enrollment.Api;

public static class EnrollmentEndpoints
{
    public static IEndpointRouteBuilder MapEnrollmentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/enrollments", async (EnrollmentCreateRequest request, EnrollmentDbContext db) =>
        {
            if (!Guid.TryParse(request.StudentId, out var studentId) || !Guid.TryParse(request.CourseId, out var courseId)
                || studentId == Guid.Empty || courseId == Guid.Empty)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["Ids"] = new[] { "StudentId and CourseId must be valid GUIDs." }
                });
            }

            // prevent duplicate insert up-front as an extra guard next to DB unique index
            var existing = await db.Enrollments.AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);
            if (existing)
            {
                return Results.Conflict(new { Message = "Student is already enrolled in this course." });
            }

            var enrollment = new Enrollment
            {
                Id = Guid.NewGuid(),
                StudentId = studentId,
                CourseId = courseId,
                Status = "Pending"
            };

            try
            {
                db.Enrollments.Add(enrollment);
                await db.SaveChangesAsync();
                return Results.Created($"/enrollments/{enrollment.Id}", enrollment);
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate") == true
                                             || ex.InnerException?.Message.Contains("unique") == true)
            {
                return Results.Conflict(new { Message = "Student is already enrolled in this course." });
            }
        })
        .WithName("CreateEnrollment")
        .WithSummary("Create a new enrollment")
        .WithDescription("Creates an enrollment in Pending state. Returns 201 on success, 409 if the student is already enrolled in the course, 400 for invalid GUIDs.")
        .Produces<Enrollment>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        app.MapGet("/enrollments/{id:guid}", async (Guid id, EnrollmentDbContext db) =>
        {
            var enrollment = await db.Enrollments.FindAsync(id);
            return enrollment is null
                ? Results.NotFound(new { Message = $"Enrollment {id} not found." })
                : Results.Ok(enrollment);
        })
        .WithName("GetEnrollmentById")
        .WithSummary("Get enrollment by id")
        .WithDescription("Returns a single enrollment by Id. 404 if not found.")
        .Produces<Enrollment>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
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
        })
        .WithName("ConfirmEnrollment")
        .WithSummary("Confirm an enrollment")
        .WithDescription("Transitions an enrollment from Pending to Confirmed. 404 if not found, 409 if not in Pending.")
        .Produces<Enrollment>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

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
        })
        .WithName("CancelEnrollment")
        .WithSummary("Cancel an enrollment")
        .WithDescription("Cancels an enrollment unless it is already Completed or Cancelled. 404 if not found, 409 on invalid state.")
        .Produces<Enrollment>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        return app;
    }
}

public record EnrollmentCreateRequest(string StudentId, string CourseId);