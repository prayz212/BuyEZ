using WarehouseWorker.Application;
using WarehouseWorker.Application.Domain;
using WarehouseWorker.Application.Infrastructure.Persistence;
using WarehouseWorker.BackgroundJobs;

using Shared.Common;
using Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.ConfigureOptions<QuartzConfigurationOptions>();
builder.Host.UseCustomSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await app.InitializeDatabaseAsync<ApplicationDbContextInitializer>(app.Environment);

app.UseHttpsRedirection();


// TODO: remove it when integrate with event sourcing
app.MapGet("/dummy", async (ApplicationDbContext context) =>
{
    var package = new Package(Guid.NewGuid().ToString());

    await context.Packages.AddAsync(package);
    await context.SaveChangesAsync();
})
.WithName("GenerateDummyPackage")
.WithOpenApi();

app.Run();
