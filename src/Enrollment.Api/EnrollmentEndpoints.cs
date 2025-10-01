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
            // For now: stubbed response
            var response = new EnrollmentResponse(
                id,
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Pending"
            );

            return Results.Ok(response);
        });

        app.MapPost("/enrollments/{id:guid}/confirm", (Guid id) =>
        {
            var reponse = new EnrollmentResponse(
                id,
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Confirmed"
            );

            return Results.Ok(reponse);
        });

        app.MapPost("/enrollments/{id:guid}/cancel", (Guid id) =>
        {
            var response = new EnrollmentResponse(
                id,
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Cancelled"
            );

            return Results.Ok(response);
        });

        return app;
    }
}

public record EnrollmentRequest(Guid StudentId, Guid CourseId);

public record EnrollmentResponse(Guid Id, Guid StudentId, Guid CourseId, string Status);