using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace BolaDemo.Api.Authentication;

/// <summary>
/// Demo-only authentication: trusts "X-Tenant-Id" and "X-User-Id" headers instead of validating
/// a real JWT or cookie. This exists so the BOLA fix in this repo is visible independent of any
/// specific auth provider. Swap this for your real authentication handler — the authorization
/// code (SameTenantAuthorizationHandler, the query filter) is what the article is actually about.
/// </summary>
public sealed class TestSchemeAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory loggerFactory,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, loggerFactory, encoder)
{
    public const string SchemeName = "TestScheme";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Tenant-Id", out var tenantId) ||
            !Request.Headers.TryGetValue("X-User-Id", out var userId))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing X-Tenant-Id or X-User-Id header."));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new("tenant_id", tenantId.ToString()),
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
