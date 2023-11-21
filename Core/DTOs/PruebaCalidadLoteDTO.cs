using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class PruebaCalidadLoteDTO
    {
        public string Id_Rollo { get; set; }
        public double Trama1 { get; set; }
        public double Trama2 { get; set; }
        public double Trama3 { get; set; }
        public double undimbre1 { get; set; }
        public double undimbre2 { get; set; }
        public double undimbre3 { get; set; }
        public double Torsion_AC { get; set; }
        public double Torsion_BD { get; set; }
    }
}
