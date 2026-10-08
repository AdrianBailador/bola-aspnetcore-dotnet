using BolaDemo.Api.Domain;

namespace BolaDemo.Api.Contracts;

public sealed record InvoiceDto(int Id, Guid TenantId, Guid OwnerId, decimal Amount, string Status)
{
    public static InvoiceDto FromEntity(Invoice invoice) =>
        new(invoice.Id, invoice.TenantId, invoice.OwnerId, invoice.Amount, invoice.Status);
}
