using System.Xml.Linq;
using MediatR;
using ApiFinanzas.Domain.Dto.Response;
using ClosedXML.Excel;
using System.Globalization;

namespace ApiFinanzas.Application.XmlProcessing;

public class ProcessXmlToExcelCommandHandler : IRequestHandler<ProcessXmlToExcelCommand, ProcessXmlToExcelResponse>
{
    private const string numericoConDecimales = "#,##0.00";

    public async Task<ProcessXmlToExcelResponse> Handle(ProcessXmlToExcelCommand request, CancellationToken cancellationToken)
    {
        string xmlContent;
        using (var reader = new StreamReader(request.File.OpenReadStream()))
        {
            xmlContent = await reader.ReadToEndAsync(cancellationToken);
        }

        var inicioColumnas = 2;
        var inicioFilas = 2;
        var finColumnas = 7;
        var finFilas = 0;

        var xdoc = XDocument.Parse(xmlContent);
        var ns = xdoc.Root!.GetDefaultNamespace();

        var de = xdoc.Descendants(ns + "DE").FirstOrDefault()
            ?? throw new Exception("No se encontró el nodo <DE> en el XML");

        var id = de
            .Attribute("Id")?
            .Value;

        var fechaEmision = de.Descendants(ns + "dFeEmiDE").FirstOrDefault()?.Value ?? "";
        var nomEmisor = de.Descendants(ns + "dNomEmi").FirstOrDefault()?.Value ?? "";
        var nomReceptor = de.Descendants(ns + "dNomRec").FirstOrDefault()?.Value ?? "";
        var totalGral = de.Descendants(ns + "dTotGralOpe").FirstOrDefault()?.Value ?? "";

        var items = de.Descendants(ns + "gCamItem")
            .Select((item, index) => new
            {
                Nro = index + 1,
                Codigo = item.Element(ns + "dCodInt")?.Value ?? "",
                Descripcion = item.Element(ns + "dDesProSer")?.Value ?? "",
                Cantidad = item.Element(ns + "dCantProSer")?.Value ?? "",
                PrecioUnitario = item.Descendants(ns + "dPUniProSer").FirstOrDefault()?.Value ?? "",
                Total = item.Descendants(ns + "dTotOpeItem").FirstOrDefault()?.Value ?? ""
            })
            .ToList();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Factura");

        var fecha = DateTime.Parse(fechaEmision, CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");
        var titulo = $"{nomEmisor} - {nomReceptor} - {fecha}";
        var subTitulo = id;

        // Título
        ws.Cell(2, 2).Value = titulo;
        var rangoTitulo = ws.Range(inicioFilas, inicioColumnas, inicioFilas, finColumnas);
        rangoTitulo.Merge();
        rangoTitulo.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        rangoTitulo.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);

        ws.Cell(3, 2).Value = subTitulo;
        var rangoSubTitulo = ws.Range(inicioFilas + 1, inicioColumnas, inicioFilas + 1, finColumnas);
        rangoSubTitulo.Merge();
        rangoSubTitulo.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        rangoSubTitulo.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);

        // Encabezados
        var inicioFilasEncabezados = inicioFilas + 2;
        var inicioColumnasEncabezados = 2;
        ws.Cell(inicioFilasEncabezados, inicioColumnasEncabezados + 0).Value = "Nro";
        ws.Cell(inicioFilasEncabezados, inicioColumnasEncabezados + 1).Value = "Descripción";
        ws.Cell(inicioFilasEncabezados, inicioColumnasEncabezados + 2).Value = "Cantidad";
        ws.Cell(inicioFilasEncabezados, inicioColumnasEncabezados + 3).Value = "Precio Unitario";
        ws.Cell(inicioFilasEncabezados, inicioColumnasEncabezados + 4).Value = "Total";
        ws.Cell(inicioFilasEncabezados, inicioColumnasEncabezados + 5).Value = "Paga";
        var rangoEncabezados = ws.Range(inicioFilasEncabezados, inicioColumnas, inicioFilasEncabezados, finColumnas);
        rangoEncabezados.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        rangoEncabezados.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);

        var row = 5;

        
        // Cuerpo
        foreach (var item in items)
        {
            inicioFilasEncabezados = inicioColumnasEncabezados;
            ws.Cell(row, inicioFilasEncabezados++).Value = item.Nro;
            ws.Cell(row, inicioFilasEncabezados++).Value = item.Descripcion;

            ws.Cell(row, inicioFilasEncabezados++).Value = decimal.TryParse(item.Cantidad, NumberStyles.Any, CultureInfo.InvariantCulture, out var cant) ? cant : 0;
            ws.Cell(row, inicioFilasEncabezados).Style.NumberFormat.Format = numericoConDecimales;

            ws.Cell(row, inicioFilasEncabezados++).Value = decimal.TryParse(item.PrecioUnitario, NumberStyles.Any, CultureInfo.InvariantCulture, out var pu) ? pu : 0;
            ws.Cell(row, inicioFilasEncabezados).Style.NumberFormat.Format = numericoConDecimales;

            ws.Cell(row, inicioFilasEncabezados++).Value = decimal.TryParse(item.Total, NumberStyles.Any, CultureInfo.InvariantCulture, out var tot) ? tot : 0;
            ws.Cell(row, inicioFilasEncabezados).Style.NumberFormat.Format = numericoConDecimales;

            row++;
            finFilas = row;
        }

        // Total final
        ws.Cell(row, finColumnas - 2).Value = "TOTAL:";
        ws.Cell(row, finColumnas - 1).Value = decimal.TryParse(totalGral.Replace('.',','), out var totalG) ? totalG : 0;
        ws.Cell(row, finColumnas - 1).Style.NumberFormat.Format = numericoConDecimales;

        // Estilo general
        ws.Columns().AdjustToContents();
        ws.RangeUsed()!.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        ws.RangeUsed()!.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        var rangoTotal = ws.Range(inicioFilas, inicioColumnas, finFilas, finColumnas);
        rangoTotal.Style.Font.Bold = true;
        rangoTotal.Style.Fill.BackgroundColor = XLColor.LightGray;

        var rangoItems = ws.Range(5, inicioColumnasEncabezados + 1, row - 1, finColumnas);
        rangoItems.Style.Font.Bold = false;

        // Al final aumentamos el ancho de la columna "Paga"
        ws.Column(inicioColumnasEncabezados + 5).Width = 15;

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);

        return new ProcessXmlToExcelResponse
        {
            Excel = ms.ToArray(),
            NombreArchivo = $"{titulo} - {subTitulo}"
        };
    }
}
