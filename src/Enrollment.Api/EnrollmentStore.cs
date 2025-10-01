namespace Enrollment.Api;

static class EnrollmentStore
{
    public static Dictionary<Guid, EnrollmentResponse> Enrollments { get; set; } = new();
}