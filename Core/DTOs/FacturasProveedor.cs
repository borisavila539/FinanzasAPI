using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class FacturasProveedor
    {
        public string CuentaDelProveedor { get; set; }
        public string NombreDelProveedor { get;set; }
        public bool EstadoEnviado { get; set; }
        public string Email { get;set; }
        public string NumeroDeFactura { get;set; }
        public decimal Debito { get;set; }
        public string LoteDeDiario { get;set; }
        public string Asiento { get;set; }
        public string Factura { get;set; }        
        public int CantidadRetenciones { get;set; }
    }
}
