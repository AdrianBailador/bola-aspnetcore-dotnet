using Microsoft.AspNetCore.Mvc.Testing;

namespace BolaDemo.Tests;

public sealed class BolaApiFactory : WebApplicationFactory<Program>
{
    public HttpClient CreateClientAs(Guid tenantId, Guid userId)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());
        client.DefaultRequestHeaders.Add("X-User-Id", userId.ToString());
        return client;
    }
}
