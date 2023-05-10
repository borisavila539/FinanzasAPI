using System.Collections.Generic;
using ReportingServices.DTOs;

namespace Core.Interfaces
{
    public interface IReportService
    {
        byte[] GenerateReport_ComprobanteRetencionProveedor(List<ComprobanteRetencionPDF_DTO> datos);
    }
}
