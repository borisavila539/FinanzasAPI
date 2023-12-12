using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class DatosExcelDTO
    {
		public string Master_ID { get; set; }
		public int Lavado { get; set; }
		public int Medida_ID { get; set; }
		public string Talla { get; set; }
		public string Spec { get; set; }
		public string Tolerancia_1{ get; set; }
		public string Tolerancia_2{ get; set; }
		public string Intruccion_1{ get; set; }
		public string Intruccion_2{ get; set; }
		public string Intruccion_3 { get; set; }
		public string Altura_Asiento { get; set; }
	}
}
