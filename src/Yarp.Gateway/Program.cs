using Shared.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Host.UseCustomSerilog();

var app = builder.Build();

app.MapReverseProxy();

app.Run();