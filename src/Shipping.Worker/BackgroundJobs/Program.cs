using ShippingWorker.BackgroundJobs;
using ShippingWorker.Application;
using ShippingWorker.Application.Domain;
using ShippingWorker.Application.Infrastructure.Persistence;
using ShippingWorker.Application.Domain.Interfaces.Repositories;

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

await app.InitializeDatabaseAsync<DbContextInitializer>(app.Environment);

app.UseHttpsRedirection();

// TODO: remove it when integrate with event sourcing
app.MapGet("/dummy", async (IShipmentRepository repository) =>
{
    var shipment = Shipment.CreateNew(Guid.NewGuid().ToString());

    await repository.AddAsync(shipment);
    await repository.SaveChangesAsync();
})
.WithName("GenerateDummyShipment")
.WithOpenApi();

app.Run();
