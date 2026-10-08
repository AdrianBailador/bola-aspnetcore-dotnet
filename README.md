# BOLA in ASP.NET Core

Companion repo for [BOLA in ASP.NET Core: Why Authenticated Users Can Still Read Someone Else's Data](https://adrianbailador.github.io/blog/73-bola-aspnetcore-object-level-authorization) — a minimal, runnable demo of Broken Object Level Authorization (OWASP API Security Top 10, #1) and the two-layer fix: an EF Core global query filter plus resource-based authorization via `IAuthorizationService`.

## The bug, in one endpoint

`GET /api/vulnerable/invoices/{id}` is authenticated (`[Authorize]`) but has no object-level check — any logged-in user who knows or guesses an id gets that invoice back, regardless of which tenant it belongs to. `InvoiceAuthorizationTests.Vulnerable_endpoint_lets_tenant_A_read_tenant_Bs_invoice` demonstrates it.

## The fix, in two layers

- **Layer 1 — `AppDbContext`'s query filter** (`Data/AppDbContext.cs`): every query against `Invoices` is scoped to the caller's tenant automatically, including queries nobody's written yet.
- **Layer 2 — `SameTenantAuthorizationHandler`** (`Authorization/`): a resource-based policy evaluated against the specific object a request named, not just the caller's identity. It's what still protects `GET /api/invoices/batch`, which deliberately bypasses layer 1 with `IgnoreQueryFilters()` — the exact "bulk endpoints are the ones everyone forgets" pitfall from the article.

Both endpoints sit side by side in `Controllers/` so you can diff them directly.

## Running it

```bash
dotnet test                                    # 5 tests, including the cross-tenant case
dotnet run --project src/BolaDemo.Api          # API on the port printed to the console
```

Authentication in this demo is a header-based test scheme (`X-Tenant-Id`, `X-User-Id`) instead of real JWT/cookie auth, so the authorization logic is visible independent of any specific auth provider. Swap in your real authentication handler — the query filter and the resource-based handler are the parts that matter.

```bash
# Tenant A reading its own invoice — 200
curl http://localhost:<port>/api/invoices/4821 -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111" -H "X-User-Id: aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"

# Tenant A reading tenant B's invoice through the vulnerable endpoint — 200 (the bug)
curl http://localhost:<port>/api/vulnerable/invoices/9031 -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111" -H "X-User-Id: aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"

# Same request through the fixed endpoint — 404, the query filter excludes the row
curl http://localhost:<port>/api/invoices/9031 -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111" -H "X-User-Id: aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"
```

## Project layout

```
src/BolaDemo.Api/
  Domain/Invoice.cs                          entity with TenantId + OwnerId
  Data/AppDbContext.cs                       layer 1: the tenant query filter
  Data/HttpContextTenantProvider.cs          resolves the caller's tenant from claims
  Authorization/SameTenantRequirement.cs     layer 2: the requirement marker
  Authorization/SameTenantAuthorizationHandler.cs
  Authentication/TestSchemeAuthenticationHandler.cs   demo-only header auth
  Controllers/VulnerableInvoicesController.cs   the "Before"
  Controllers/InvoicesController.cs             the "After" (single + batch)
tests/BolaDemo.Tests/
  InvoiceAuthorizationTests.cs                5 tests covering the vulnerable path,
                                               the fix, and the batch-endpoint pitfall
```

## License

MIT — use this however's useful.
