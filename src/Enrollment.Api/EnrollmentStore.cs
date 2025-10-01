namespace Enrollment.Api;

public static class EnrollmentStore
{
    private static readonly Dictionary<Guid, EnrollmentResponse> Enrollments = new();

    public static EnrollmentResponse Add(Guid studentId, Guid courseId)
    {
        var id = Guid.NewGuid();
        var response = new EnrollmentResponse(id, studentId, courseId, "Pending");
        Enrollments[id] = response;
        return response;
    }
    
    public static EnrollmentResponse? Get(Guid id) =>
        Enrollments.TryGetValue(id, out var enrollment) ? enrollment : null;
    
    public static EnrollmentResponse? Confirm(Guid id)
    {
        if (!Enrollments.TryGetValue(id, out var enrollment)) return null;

        if (enrollment.Status != "Pending")
        {
            return enrollment with { Status = "Conflict" }; // marker for invalid transition
        }

        var updated = enrollment with { Status = "Confirmed" };
        Enrollments[id] = updated;
        return updated;
    }

    public static EnrollmentResponse? Cancel(Guid id)
    {
        if (!Enrollments.TryGetValue(id, out var enrollment)) return null;

        if (enrollment.Status is "Completed" or "Cancelled")
        {
            return enrollment with { Status = "Conflict" };
        }

        var updated = enrollment with { Status = "Cancelled" };
        Enrollments[id] = updated;
        return updated;
    }
}