using BolaDemo.Api.Domain;
using Microsoft.AspNetCore.Authorization;

namespace BolaDemo.Api.Authorization;

/// <summary>
/// Layer 2 of the BOLA fix: an object-level check, evaluated against the specific resource
/// a request named, not just the caller's identity. Reusable across every endpoint that
/// returns an Invoice, instead of a tenantId == invoice.TenantId check repeated ad hoc.
/// </summary>
public sealed class SameTenantAuthorizationHandler : AuthorizationHandler<SameTenantRequirement, Invoice>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SameTenantRequirement requirement,
        Invoice resource)
    {
        var tenantClaim = context.User.FindFirst("tenant_id")?.Value;

        if (Guid.TryParse(tenantClaim, out var tenantId) && tenantId == resource.TenantId)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
