using BolaDemo.Api.Authentication;
using BolaDemo.Api.Authorization;
using BolaDemo.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentTenantProvider, HttpContextTenantProvider>();

builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("BolaDemo"));

builder.Services
    .AddAuthentication(TestSchemeAuthenticationHandler.SchemeName)
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestSchemeAuthenticationHandler>(
        TestSchemeAuthenticationHandler.SchemeName, _ => { });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("SameTenant", policy => policy.Requirements.Add(new SameTenantRequirement()));

builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, SameTenantAuthorizationHandler>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    SeedData.Seed(dbContext);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
