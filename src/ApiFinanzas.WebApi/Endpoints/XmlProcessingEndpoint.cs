using ApiFinanzas.Application.XmlProcessing;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApiFinanzas.WebApi.Endpoints;

public static class XmlProcessingEndpoint
{
    public static void AddXmlProcessingEndpoints(this WebApplication app)
    {
        app.MapPost("xml-to-excel", XmlToExcelAsync)
        .WithTags("XmlProcessing");
    }

    private static async Task<IResult> XmlToExcelAsync(HttpRequest request, IMediator mediator)
    {
        var form = await request.ReadFormAsync();
        var file = form.Files.GetFile("File");

        if (file is null)
            return Results.BadRequest("Archivo XML no encontrado en 'File'.");

        var command = new ProcessXmlToExcelCommand
        {
            File = file
        };

        var result = await mediator.Send(command);
        return Results.File(
            fileContents: result.Excel,
            contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileDownloadName: $"{result.NombreArchivo}.xlsx"
        );
    }
}