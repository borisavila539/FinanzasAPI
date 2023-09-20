using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
   public class InsertarAnchoYardasDTO
    {
        public int Id_Rollo { get; set; }
        public decimal Ancho_1 { get; set; }
        public decimal Ancho_2 { get; set; }
        public decimal Ancho_3 { get; set; }
        public decimal Yardas_Proveedor { get; set; }
        public decimal Yardas_Reales { get; set; }
        public decimal Diferencia_Yardas { get; set; }
        public string Observaciones { get; set; }

    }
}
