namespace ApiFinanzas.Domain.Dto.Response;

public class ProcessXmlToExcelResponse
{
    public byte[] Excel { get; set; } = default!;
    public string NombreArchivo { get; set; } = default!;
}