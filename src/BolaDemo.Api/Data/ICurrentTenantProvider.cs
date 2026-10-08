namespace BolaDemo.Api.Data;

public interface ICurrentTenantProvider
{
    Guid? TenantId { get; }
}
