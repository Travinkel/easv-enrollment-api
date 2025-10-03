using Microsoft.EntityFrameworkCore;

namespace Enrollment.Api;

public class EnrollmentDbContext : DbContext
{
    public EnrollmentDbContext(DbContextOptions<EnrollmentDbContext> options) : base(options) {}
    
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Enrollment>()
            .Property(e => e.Version)
            .IsConcurrencyToken();
        
        modelBuilder.Entity<Enrollment>()
            .HasIndex(e => new { e.StudentId, e.CourseId });
    }
}

public class Enrollment
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public string Status { get; set; } = "Pending";
    public int Version { get; set; } // optimistic concurrency
    
    
}