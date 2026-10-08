using BolaDemo.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace BolaDemo.Api.Data;

public static class SeedData
{
    public static readonly Guid TenantA = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid TenantB = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static readonly Guid UserAlice = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid UserBob = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    public static void Seed(AppDbContext dbContext)
    {
        if (!dbContext.Database.IsInMemory())
            return;

        // No HttpContext at startup means the query filter's tenant is null, which matches
        // nothing — IgnoreQueryFilters() is required here just to check whether seeding
        // already ran (the in-memory store is shared by name across WebApplicationFactory
        // instances within a test run, so this guard keeps re-seeding from throwing).
        if (dbContext.Invoices.IgnoreQueryFilters().Any())
            return;

        dbContext.Invoices.AddRange(
            new Invoice { Id = 4821, TenantId = TenantA, OwnerId = UserAlice, Amount = 1250.00m, Status = "Overdue" },
            new Invoice { Id = 4822, TenantId = TenantA, OwnerId = UserAlice, Amount = 340.50m, Status = "Paid" },
            new Invoice { Id = 9031, TenantId = TenantB, OwnerId = UserBob, Amount = 980.00m, Status = "Overdue" }
        );
        dbContext.SaveChanges();
    }
}
