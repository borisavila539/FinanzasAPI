using AspNetCore.Reporting;
using Core.Interfaces;
using Infraestructure.NumberToText;
using ReportingServices.DTOs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace FinanzasAPI.Reporting
{
    public class Reporting_Services : IReportService
    {
        public string filePathReport { get; set; }
        private RenderType getRenderType(string reportType)
        {
            var renderType = RenderType.Pdf;
            renderType = reportType.ToUpper() switch
            {
                "XLS" => RenderType.Excel,
                "WORD" => RenderType.Word,
                "PDF" => RenderType.Pdf,
            };
            return renderType;
        }
        

        public byte[] GenerateReport_ComprobanteRetencionProveedor( List<ComprobanteRetencionPDF_DTO> datos)
        {
            string filePath = Assembly.GetExecutingAssembly().Location.Replace("FinanzasAPI.dll", string.Empty);
            string rdlcFilesPath = string.Format("Reporting\\Reports\\ComprobanteRetencionProveedor.rdlc", filePath);

            //pruebas {0}..\\..\\..\\Reporting\\Reports\\ComprobanteRetencionProveedor.rdlc
            //produccion \\Reporting\\Reports\\ComprobanteRetencionProveedor.rdlc

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding.GetEncoding("utf-8");
            LocalReport report = new LocalReport(rdlcFilesPath);

            List<ComprobanteRetencionPDF_DTO> comprobantes = new List<ComprobanteRetencionPDF_DTO>();
            ComprobanteRetencionPDF_DTO comprobante = new ComprobanteRetencionPDF_DTO();

            decimal sumaRetension = 0;
            decimal total = 0;

            foreach (var item in datos)
            {
                comprobante.header02 = item.header02;
                comprobante.header03 = item.header03;
                comprobante.telefono = item.telefono;
                comprobante.correoElectronico = item.correoElectronico;
                comprobante.WebSite = item.WebSite;
                comprobante.comprobante = item.comprobante;
                comprobante.empresaCAI = item.empresaCAI;
                comprobante.rango = item.rango;
                comprobante.fechaEmision = item.fechaEmision;
                comprobante.fechaLimiteEmision = item.fechaLimiteEmision;
                comprobante.middle01NombreEmpresa = item.middle01NombreEmpresa;
                comprobante.middle01RTNEmpresa = item.middle01RTNEmpresa;
                comprobante.middle02 = item.middle02;
                comprobante.middle03 = item.middle03;
                comprobante.proveedor = item.proveedor;
                comprobante.proveedorRTN = item.proveedorRTN;
                comprobante.fechaFactura = item.fechaFactura;
                comprobante.numeroFactura = item.numeroFactura;
                comprobante.proveedorCAI = item.proveedorCAI;
                comprobante.importe = item.importe;
                comprobante.retencionISR = item.retencionISR;
                comprobante.footer01 = item.footer01;
                comprobante.footer02 = item.footer02;
                comprobante.footer03 = item.footer03;
                comprobantes.Add(comprobante);
                comprobante = new ComprobanteRetencionPDF_DTO();

                total = Convert.ToDecimal(item.retencionISR);
                sumaRetension += total;
                
            }

            sumaRetension -= total;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            NumberToText  numberToText = new NumberToText();
            string texto = numberToText.EnLetras(Convert.ToString(sumaRetension));

            texto = char.ToUpper(texto[0] ) + texto.Substring(1);

            parameters.Add("ValorRetenido", sumaRetension.ToString("0,0.00", CultureInfo.InvariantCulture));
            parameters.Add("NumberToText", texto );
            var ret = new LocalReport(rdlcFilesPath);
            report.AddDataSource("ComprobanteRetencion", comprobantes);
            try{
                int exte = (int)(DateTime.Now.Ticks >> 1);
                var result = report.Execute(getRenderType("PDF"), exte, parameters);
                filePathReport = rdlcFilesPath;
                return result.MainStream;
            }catch (Exception ex)
            {

            }

            filePathReport = rdlcFilesPath;

            return ret.Execute(getRenderType("PDF")).MainStream;

            
        }
    }
}
