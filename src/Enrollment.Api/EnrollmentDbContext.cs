using Microsoft.EntityFrameworkCore;

namespace Enrollment.Api;

public class EnrollmentDbContext : DbContext
{
    public EnrollmentDbContext(DbContextOptions<EnrollmentDbContext> options) : base(options) {}
    
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
}

public class Enrollment
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public string Status { get; set; } = "Pending";
    public int Version { get; set; } // optimistic concurrency
}