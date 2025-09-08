using ApiMarangatu.Domain.Dto.Response;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ApiMarangatu.Application.XmlProcessing;

public class ProcessXmlToExcelCommand : IRequest<ProcessXmlToExcelResponse> 
{
        public IFormFile File { get; set; } = default!;
}