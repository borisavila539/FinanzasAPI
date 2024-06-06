using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class Factura
    {
        public string CuentaDelProveedor { get; set; }
        public string NumeroDeFactura { get; set; }
        public int CantidadRetenciones { get; set; }
        public DateTime FechaRetencion { get; set; }
    }
}
