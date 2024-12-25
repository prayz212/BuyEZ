using Identity.Apis.Filters;
using Identity.Application;
using Identity.Application.Extensions;

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

builder.Services.AddSwaggerGen();

builder.Services.AddProblemDetails();

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    await app.InitializeDatabaseAsync();
}

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
