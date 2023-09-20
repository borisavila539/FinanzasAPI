using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class ObtenerDatosDefectosDTO
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
        public Boolean Activo { get; set; }
        public int Nivel_1 { get; set; }
        public int Nivel_2 { get; set; }
        public int Nivel_3 { get; set; }
        public int Nivel_4 { get; set; }
        public int Total_Defectos { get; set; }
        public string Observaciones { get; set; }


    }
}
