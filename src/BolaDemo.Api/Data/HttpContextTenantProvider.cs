namespace BolaDemo.Api.Data;

public sealed class HttpContextTenantProvider(IHttpContextAccessor httpContextAccessor) : ICurrentTenantProvider
{
    public Guid? TenantId
    {
        get
        {
            var claim = httpContextAccessor.HttpContext?.User.FindFirst("tenant_id")?.Value;
            return Guid.TryParse(claim, out var tenantId) ? tenantId : null;
        }
    }
}
