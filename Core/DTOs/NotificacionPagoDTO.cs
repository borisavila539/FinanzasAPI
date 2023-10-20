using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class NotificacionPagoDTO
    {
        public DateTime fecha { get; set; }
        public string? correo { get; set; }
        public string proveedorNum { get; set; }
        public string NombreProveedor { get; set; }
        public string Nombrebanco { get; set; }
        public string Cuenta { get; set; }
        public string NumeroFactura { get; set; }
        public string divisa { get; set; }
        public double monto { get; set; }
        public string empresa { get; set; }
        public string paymmode { get; set; }
    }
}
