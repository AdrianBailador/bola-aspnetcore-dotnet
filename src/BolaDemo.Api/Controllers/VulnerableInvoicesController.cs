using BolaDemo.Api.Contracts;
using BolaDemo.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BolaDemo.Api.Controllers;

/// <summary>
/// The "Before" from the article: authenticated, but with no object-level check.
/// IgnoreQueryFilters() here stands in for a codebase that predates the tenant
/// query filter, or a repository method copy-pasted from one that legitimately
/// needs it. Compare with InvoicesController.
/// </summary>
[Authorize]
[ApiController]
[Route("api/vulnerable/invoices")]
public sealed class VulnerableInvoicesController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet("{invoiceId:int}")]
    public async Task<ActionResult<InvoiceDto>> GetInvoice(int invoiceId, CancellationToken ct)
    {
        var invoice = await dbContext.Invoices
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(i => i.Id == invoiceId, ct);

        if (invoice is null)
            return NotFound();

        // Bug: whoever asked for invoiceId 9031 gets invoice 9031, regardless of tenant.
        return Ok(InvoiceDto.FromEntity(invoice));
    }
}
