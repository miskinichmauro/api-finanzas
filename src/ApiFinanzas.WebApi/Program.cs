using System.Reflection;
using ApiFinanzas.WebApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.Load("ApiFinanzas.Application"));
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


var app = builder.Build();

app.AddXmlProcessingEndpoints();
if (!app.Environment.IsDevelopment())
{
    app.MapGet("/", () => "¡Bienvenido a la API de Finanzas!");
}
app.UseCors();

await app.RunAsync();
