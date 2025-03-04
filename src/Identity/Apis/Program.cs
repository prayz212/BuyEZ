using Identity.Application;
using Identity.Application.Infrastructure.Persistence;

using Shared.Common;
using Shared.Filters;
using Shared.Extensions;
using Shared.Middlewares;

using ProtoBuf.Grpc.Server;

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerConfiguration();

    app.MapCodeFirstGrpcReflectionService();

    await app.InitializeDatabaseAsync<ApplicationDbContextInitializer>();
}

app.UseMiddleware<AuthorizationFailureMiddleware>();
app.UseMiddleware<ExtractTokenMiddleware>();

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGrpcServices();

app.MapControllers();

app.Run();
