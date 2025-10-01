using System.Reflection.Metadata.Ecma335;

namespace Enrollment.Api;

public static class EnrollmentEndpoints
{
    public static IEndpointRouteBuilder MapEnrollmentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/enrollments", (EnrollmentRequest request) =>
        {
            if (request.StudentId == Guid.Empty || request.CourseId == Guid.Empty)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["Ids"] = new[] { "StudentId and CourseId must be valid GUIDs." }
                });
            }
            var enrollmentId = Guid.NewGuid();
            var response = new EnrollmentResponse(
                enrollmentId,
                request.StudentId,
                request.CourseId,
                "Pending"
            );

            return Results.Created($"/enrollments/{enrollmentId}", response);
        });

        app.MapGet("/enrollments/{id:guid}", (Guid id) =>
        {
            var enrollment = EnrollmentDb.Get(id);

            return enrollment is null
                ? Results.NotFound(new { Message = $"Enrollment {id} not found." })
                : Results.Ok(enrollment);
        });

        app.MapPost("/enrollments/{id:guid}/confirm", (Guid id) =>
        {
            var result = EnrollmentDb.Confirm(id);
            if (result is null) return Results.NotFound();

            return result.Status == "Conflict"
                ? Results.Conflict(new { Message = "Enrollment not in Pending state." })
                : Results.Ok(result);
        });

        app.MapPost("/enrollments/{id:guid}/cancel", (Guid id) =>
        {
            var result = EnrollmentDb.Cancel(id);
            if (result is null) return Results.NotFound();

            return result.Status == "Conflict"
                ? Results.Conflict(new { Message = "Enrollment already completed/cancelled." })
                : Results.Ok(result);
        });

        return app;
    }
}

public record EnrollmentRequest(Guid StudentId, Guid CourseId);

public record EnrollmentResponse(Guid Id, Guid StudentId, Guid CourseId, string Status);