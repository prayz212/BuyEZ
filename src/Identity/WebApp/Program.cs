using Identity.Application;
using Identity.Application.Extensions;
using Identity.Application.Infrastructure.Persistence;

using Shared.Common;
using Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.ConfigureService();
builder.Services.AddCustomIdentity(builder.Configuration);
builder.Services.AddCustomIdentityServer(builder.Configuration);
builder.Host.UseCustomSerilog();

var app = builder.Build();

// Ensure table and data been migrated and seeded
await app.InitializeDatabaseAsync<PersistedGrantDbContextInitializer>(app.Environment);
await app.InitializeDatabaseAsync<ConfigurationDbContextInitializer>(app.Environment);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseIdentityServer();

app.UseAuthorization();

app.MapRazorPages()
    .RequireAuthorization();

app.Run();
