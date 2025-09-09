using ApiFinanzas.WebApi.Endpoints;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Finanzas", Version = "v1" });
});

var app = builder.Build();

app.AddXmlProcessingEndpoints();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Finanzas v1");
        c.RoutePrefix = string.Empty;
    });
}

app.MapGet("/", () => "¡Bienvenido a la API de Finanzas!");

app.Run();
