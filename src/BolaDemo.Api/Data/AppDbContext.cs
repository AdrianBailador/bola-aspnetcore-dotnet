using BolaDemo.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace BolaDemo.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options, ICurrentTenantProvider tenantProvider)
    : DbContext(options)
{
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Layer 1: structural tenant isolation. Every query against Invoices is scoped
        // to the caller's tenant automatically, including ones nobody's written yet.
        // IgnoreQueryFilters() disables this per call site — see VulnerableInvoicesController
        // for what that looks like when it's the ONLY control, and InvoicesController for
        // why a per-object check (layer 2) still matters even with this filter in place.
        modelBuilder.Entity<Invoice>()
            .HasQueryFilter(invoice => invoice.TenantId == tenantProvider.TenantId);
    }
}
