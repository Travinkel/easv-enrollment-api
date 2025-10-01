using System.Reflection.Metadata.Ecma335;

namespace Enrollment.Api;

public static class EnrollmentEndpoints
{
    public static IEndpointRouteBuilder MapEnrollmentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/enrollments", (EnrollmentRequest request) =>
        {
            if (request.StudentId == Guid.Empty || request.CourseId == Guid.Empty)
                return Results.ValidationProblem(new() { ["Ids"] = new[] { "StudentId and CourseId must be valid GUIDs." }});

            var created = EnrollmentStore.Add(request.StudentId, request.CourseId);
            return Results.Created($"/enrollments/{created.Id}", created);
        });

// Get by id
        app.MapGet("/enrollments/{id:guid}", (Guid id) =>
            EnrollmentStore.Get(id) is { } e ? Results.Ok(e) : Results.NotFound());

// Confirm
        app.MapPost("/enrollments/{id:guid}/confirm", (Guid id) =>
        {
            var result = EnrollmentStore.Confirm(id);
            if (result is null) return Results.NotFound();
            return result.Status == "Conflict" ? Results.Conflict(new { Message = "Invalid state" }) : Results.Ok(result);
        });

// Cancel
        app.MapPost("/enrollments/{id:guid}/cancel", (Guid id) =>
        {
            var result = EnrollmentStore.Cancel(id);
            if (result is null) return Results.NotFound();
            return result.Status == "Conflict" ? Results.Conflict(new { Message = "Invalid state" }) : Results.Ok(result);
        });

        return app;
    }
}

public record EnrollmentRequest(Guid StudentId, Guid CourseId);

public record EnrollmentResponse(Guid Id, Guid StudentId, Guid CourseId, string Status);