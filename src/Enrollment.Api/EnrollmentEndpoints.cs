using System.Reflection.Metadata.Ecma335;

namespace Enrollment.Api;

public static class EnrollmentEndpoints
{
    public static IEndpointRouteBuilder MapEnrollmentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/enrollments", (EnrollmentRequest request) =>
        {
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

        return app;
    }
}

public record EnrollmentRequest(Guid StudentId, Guid CourseId);
public record EnrollmentResponse(Guid Id, Guid StudentId, Guid CourseId, string Status);
