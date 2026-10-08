namespace BolaDemo.Api.Domain;

public sealed class Invoice
{
    public int Id { get; init; }
    public required Guid TenantId { get; init; }
    public required Guid OwnerId { get; init; }
    public required decimal Amount { get; init; }
    public required string Status { get; init; }
}
