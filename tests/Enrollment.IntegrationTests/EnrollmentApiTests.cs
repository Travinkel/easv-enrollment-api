using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;


namespace Enrollment.IntegrationTests;

public class ApiFactory : WebApplicationFactory<Program> {}

public record EnrollmentRequest(Guid StudentId, Guid CourseId);
public record EnrollmentResponse(Guid Id, Guid StudentId, Guid CourseId, string Status);

public class EnrollmentApiTests : IntegrationTestBase
{
    public EnrollmentApiTests(ApiFactory factory) : base(factory) { }
    
    [Fact]
    public async Task Post_Enrollments_Creates_201_With_Location()
    {
        var req = new EnrollmentRequest(Guid.NewGuid(), Guid.NewGuid());
        var res = await Client.PostAsJsonAsync("/enrollments", req);
        res.StatusCode.Should().Be(HttpStatusCode.Created);
        res.Headers.Location.Should().NotBeNull();
        var body = await res.Content.ReadFromJsonAsync<EnrollmentResponse>(Json);
        body!.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task Get_Enrollment_Returns_200_After_Create()
    {
        var req = new EnrollmentRequest(Guid.NewGuid(), Guid.NewGuid());
        var created = await Client.PostAsJsonAsync("/enrollments", req);
        var createdBody = await created.Content.ReadFromJsonAsync<EnrollmentResponse>(Json);

        var get = await Client.GetAsync($"/enrollments/{createdBody!.Id}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);
        var got = await get.Content.ReadFromJsonAsync<EnrollmentResponse>(Json);
        got!.Id.Should().Be(createdBody.Id);
    }

    [Fact]
    public async Task Confirm_After_Cancel_Returns_409_Conflict()
    {
        var req = new EnrollmentRequest(Guid.NewGuid(), Guid.NewGuid());
        var created = await Client.PostAsJsonAsync("/enrollments", req);
        var e = await created.Content.ReadFromJsonAsync<EnrollmentResponse>(Json);

        var cancel = await Client.PostAsync($"/enrollments/{e!.Id}/cancel", null);
        cancel.StatusCode.Should().Be(HttpStatusCode.OK);

        var confirm = await Client.PostAsync($"/enrollments/{e.Id}/confirm", null);
        confirm.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
