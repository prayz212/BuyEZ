using ClientManagementAPI.Application;
using ClientManagementAPI.Application.Infrastructure.Persistence;

using Shared.Common;
using Shared.Filters;
using Shared.Middlewares;
using Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options => 
{
    options.Filters.Add<ApiExceptionFilterAttribute>();
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options => options.AddDefaultPolicy(
    policy => policy.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()));

builder.Services.AddSwaggerGeneration();
builder.Services.AddApiVersioningConfiguration();

builder.Services.AddProblemDetails();

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Host.UseCustomSerilog();

builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerConfiguration();

    await app.InitializeDatabaseAsync<ApplicationDbContextInitializer>();
}

app.UseMiddleware<AuthorizationFailureMiddleware>();
app.UseMiddleware<ExtractTokenMiddleware>();

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
