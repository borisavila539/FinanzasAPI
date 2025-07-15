using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class ComprobanteRetencionDTO
    {
        public string proveedorNum { get; set; }
        public decimal header02 { get; set; }
        public string header03 { get; set; }
        public string telefono { get; set; }
        public string correoElectronico { get; set; }
        public string WebSite { get; set; }
        public string comprobante { get; set; }
        public string empresaCAI { get; set; }
        public string rango { get; set; }
        public DateTime fechaEmision { get; set; }
        public DateTime fechaLimiteEmision { get; set; }
        public string middle01NombreEmpresa { get; set; }
        public string middle01RTNEmpresa { get; set; }
        public string middle02 { get; set; }
        public decimal middle03 { get; set; }
        public string proveedor { get; set; }
        public string proveedorRTN { get; set; }
        public DateTime fechaFactura { get; set; }
        public string numeroFactura { get; set; }
        public string proveedorCAI { get; set; }
        public decimal importe { get; set; }
        public decimal retencionISR { get; set; }
        public string footer01 { get; set; }
        public string footer02 { get; set; }
        public string footer03 { get; set; }

        /*Commented by spineda on july/11/2025 - Begin*/
        public decimal exchRate { get; set; }
        public string currencyTxt { get;set; }
        public string currencyCode { get; set; }
        /*Commented by spineda on july/11/2025 - End*/
    }
}