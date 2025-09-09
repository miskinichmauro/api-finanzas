using System.Reflection;
using ApiFinanzas.WebApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.Load("ApiFinanzas.Application"));
});

var app = builder.Build();

app.AddXmlProcessingEndpoints();
if (!app.Environment.IsDevelopment())
{
    app.MapGet("/", () => "¡Bienvenido a la API de Finanzas!");
}

await app.RunAsync();
