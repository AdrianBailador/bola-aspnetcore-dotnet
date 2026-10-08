using BolaDemo.Api.Contracts;
using BolaDemo.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BolaDemo.Api.Controllers;

/// <summary>
/// The "After" from the article: the query filter (layer 1) scopes every query to the
/// caller's tenant, and the resource-based policy (layer 2) makes the per-object decision
/// explicit and testable instead of implicit in "the filter will have handled it."
/// </summary>
[Authorize]
[ApiController]
[Route("api/invoices")]
public sealed class InvoicesController(AppDbContext dbContext, IAuthorizationService authorizationService)
    : ControllerBase
{
    [HttpGet("{invoiceId:int}")]
    public async Task<ActionResult<InvoiceDto>> GetInvoice(int invoiceId, CancellationToken ct)
    {
        // Layer 1: the DbContext query filter already excludes other tenants' rows here,
        // so a cross-tenant id simply isn't found — this returns 404, not 403.
        var invoice = await dbContext.Invoices.FirstOrDefaultAsync(i => i.Id == invoiceId, ct);
        if (invoice is null)
            return NotFound();

        // Layer 2: explicit, reusable, and the control that still matters anywhere this
        // query is ever written with IgnoreQueryFilters() — see GetInvoicesBatch below.
        var authResult = await authorizationService.AuthorizeAsync(User, invoice, "SameTenant");
        if (!authResult.Succeeded)
            return Forbid();

        return Ok(InvoiceDto.FromEntity(invoice));
    }

    /// <summary>
    /// The bulk/export endpoint from the "Pitfalls in Production" section: it needs
    /// IgnoreQueryFilters() because it also surfaces invoices for tenants the caller's
    /// account has been granted temporary cross-tenant support access to (not modeled in
    /// this demo). Because layer 1 is bypassed here, every item gets an explicit layer-2
    /// check instead of relying on the filter — the two controls are independent, and this
    /// endpoint is exactly the call site that would otherwise leak silently.
    /// </summary>
    [HttpGet("batch")]
    public async Task<ActionResult<IReadOnlyList<InvoiceDto>>> GetInvoicesBatch(
        [FromQuery] int[] ids,
        CancellationToken ct)
    {
        var invoices = await dbContext.Invoices
            .IgnoreQueryFilters()
            .Where(i => ids.Contains(i.Id))
            .ToListAsync(ct);

        var authorized = new List<InvoiceDto>();
        foreach (var invoice in invoices)
        {
            var authResult = await authorizationService.AuthorizeAsync(User, invoice, "SameTenant");
            if (authResult.Succeeded)
                authorized.Add(InvoiceDto.FromEntity(invoice));
        }

        return Ok(authorized);
    }
}
