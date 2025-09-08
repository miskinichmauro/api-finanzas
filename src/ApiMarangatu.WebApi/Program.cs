using System.Reflection;
using ApiMarangatu.WebApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.Load("ApiMarangatu.Application"));
});

var app = builder.Build();

app.AddXmlProcessingEndpoints();

await app.RunAsync();
