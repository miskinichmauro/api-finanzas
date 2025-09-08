using ApiFinanzas.Domain.Dto.Response;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ApiFinanzas.Application.XmlProcessing;

public class ProcessXmlToExcelCommand : IRequest<ProcessXmlToExcelResponse> 
{
        public IFormFile File { get; set; } = default!;
}