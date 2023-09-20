using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class DatosRollosInsertDTOs
    {
        public int Id_Auditor_Creacion { get; set; }
        public int Id_Rollo { get; set; }
        public int Id_Estado { get; set; }
        public int Id_Defecto { get; set; }
        public int Total_Defectos { get; set; }
        public int Nivel_1 { get; set; }
        public int Nivel_2 { get; set; }
        public int Nivel_3 { get; set; }
        public int Nivel_4 { get; set; }
    }
}
