using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing; 

namespace Enrollment.IntegrationTests;

public class EnrollmentEdgeCaseTests : IntegrationTestBase
{
    public EnrollmentEdgeCaseTests(ApiFactory factory) : base(factory) { }
    [Fact]
    public async Task Duplicate_Insert_Returns_409_Conflict()
    {
        var student = Guid.NewGuid();
        var course = Guid.NewGuid();
        var req = new EnrollmentRequest(student, course);
        var res1 = await Client.PostAsJsonAsync("/enrollments", req);
        res1.StatusCode.Should().Be(HttpStatusCode.Created);

        // second insert with same pair should conflict (unique index)
        var res2 = await Client.PostAsJsonAsync("/enrollments", req);
        res2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Theory]
    [InlineData("not-a-guid", "baddata-123")] // both invalid
    [InlineData("00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000")] // empty guids
    public async Task Bad_Guids_Return_400_With_Validation(string studentRaw, string courseRaw)
    {
        // post raw json to bypass client-side Guid parsing
        var payload = new
        {
            StudentId = studentRaw,
            CourseId = courseRaw
        };

        var res = await Client.PostAsJsonAsync("/enrollments", payload);
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await res.Content.ReadFromJsonAsync<Dictionary<string, object>>(Json);
        problem!.Should().ContainKey("errors");
        var errors = problem["errors"] as JsonElement?;
        errors.HasValue.Should().BeTrue();
        errors!.Value.ToString().Should().Contain("Ids");
    }
}
