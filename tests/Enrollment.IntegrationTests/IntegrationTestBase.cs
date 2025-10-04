using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Enrollment.IntegrationTests;

public class IntegrationTestBase : IClassFixture<ApiFactory>
{
    protected readonly HttpClient Client;
    protected readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    protected IntegrationTestBase(ApiFactory factory)
    {
        Client = factory.CreateClient();
    }
}