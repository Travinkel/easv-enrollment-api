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

    [Fact]
    public async Task Confirm_Twice_Returns_409_Conflict()
    {
        var req = new EnrollmentRequest(Guid.NewGuid(), Guid.NewGuid());
        var created = await Client.PostAsJsonAsync("/enrollments", req);
        var enrollment = await created.Content.ReadFromJsonAsync<EnrollmentResponse>(Json);
        
        // First confirm + OK
        var firstConfirm = await Client.PostAsync($"/enrollments/{enrollment!.Id}/confirm", null);
        firstConfirm.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Second confirm + Conflict
        var secondConfirm = await Client.PostAsync($"/enrollments/{enrollment.Id}/confirm", null);
        secondConfirm.StatusCode.Should().Be(HttpStatusCode.Conflict); 
    }

    [Fact]
    public async Task Cancel_Twice_Returns_409_Conflict()
    {
        var req = new EnrollmentRequest(Guid.NewGuid(), Guid.NewGuid());
        var created = await Client.PostAsJsonAsync("/enrollments", req);
        var enrollment = await created.Content.ReadFromJsonAsync<EnrollmentResponse>(Json);
        
        // First cancel + OK
        var firstCancel = await Client.PostAsync($"/enrollments/{enrollment!.Id}/cancel", null);
        firstCancel.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Second cancel + Conflict
        var secondCancel = await Client.PostAsync($"/enrollments/{enrollment.Id}/cancel", null);
        secondCancel.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Confirm_After_Cancel_Returns_409_Conflict()
    {
        var req = new EnrollmentRequest(Guid.NewGuid(), Guid.NewGuid());
        var created = await Client.PostAsJsonAsync("/enrollments", req);
        var enrollment = await created.Content.ReadFromJsonAsync<EnrollmentResponse>(Json);
        
        // First cancel + OK
        var cancel = await Client.PostAsync($"/enrollments/{enrollment!.Id}/cancel", null);
        cancel.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Second confirm + Conflict
        var confirm = await Client.PostAsync($"/enrollments/{enrollment.Id}/confirm", null);
        confirm.StatusCode.Should().Be(HttpStatusCode.Conflict);
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
