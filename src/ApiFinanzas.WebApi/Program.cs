using System.Reflection;
using ApiFinanzas.WebApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.Load("ApiFinanzas.Application"));
});

var app = builder.Build();

app.AddXmlProcessingEndpoints();

await app.RunAsync();
