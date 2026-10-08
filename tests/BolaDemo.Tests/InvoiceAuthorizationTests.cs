using System.Net;
using System.Net.Http.Json;
using BolaDemo.Api.Contracts;
using BolaDemo.Api.Data;

namespace BolaDemo.Tests;

public sealed class InvoiceAuthorizationTests : IClassFixture<BolaApiFactory>
{
    private readonly BolaApiFactory _factory;

    public InvoiceAuthorizationTests(BolaApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Vulnerable_endpoint_lets_tenant_A_read_tenant_Bs_invoice()
    {
        // This is the BOLA bug: a valid, authenticated tenant A session reads data it
        // was never granted access to, because the endpoint never checks ownership.
        var client = _factory.CreateClientAs(SeedData.TenantA, SeedData.UserAlice);

        var response = await client.GetAsync("/api/vulnerable/invoices/9031");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var invoice = await response.Content.ReadFromJsonAsync<InvoiceDto>();
        Assert.Equal(SeedData.TenantB, invoice!.TenantId);
    }

    [Fact]
    public async Task Fixed_endpoint_returns_not_found_for_another_tenants_invoice()
    {
        // The query filter scopes the query itself, so a cross-tenant id is never found.
        var client = _factory.CreateClientAs(SeedData.TenantA, SeedData.UserAlice);

        var response = await client.GetAsync("/api/invoices/9031");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Fixed_endpoint_returns_own_tenants_invoice()
    {
        var client = _factory.CreateClientAs(SeedData.TenantA, SeedData.UserAlice);

        var response = await client.GetAsync("/api/invoices/4821");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var invoice = await response.Content.ReadFromJsonAsync<InvoiceDto>();
        Assert.Equal(4821, invoice!.Id);
    }

    [Fact]
    public async Task Batch_endpoint_bypasses_the_filter_but_the_resource_check_still_excludes_other_tenants()
    {
        // This endpoint deliberately uses IgnoreQueryFilters() (see InvoicesController),
        // so layer 1 is off. If layer 2 (the resource-based handler) were missing too,
        // tenant A would get tenant B's invoice back in the list — this is the regression
        // test for the "bulk endpoints are the ones everyone forgets" pitfall.
        var client = _factory.CreateClientAs(SeedData.TenantA, SeedData.UserAlice);

        var response = await client.GetAsync("/api/invoices/batch?ids=4821&ids=4822&ids=9031");
        response.EnsureSuccessStatusCode();

        var invoices = await response.Content.ReadFromJsonAsync<List<InvoiceDto>>();
        Assert.NotNull(invoices);
        Assert.Equal(2, invoices!.Count);
        Assert.All(invoices, i => Assert.Equal(SeedData.TenantA, i.TenantId));
        Assert.DoesNotContain(invoices, i => i.Id == 9031);
    }

    [Fact]
    public async Task Requests_without_authentication_headers_are_rejected()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/invoices/4821");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
