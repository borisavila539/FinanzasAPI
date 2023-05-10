using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class DatosMedidasDtos
    {
        public int id { get; set; }
        public string Nombre { get; set; }
        public string Link { get; set; }
        public Boolean activo { get; set; }
        public string intruccion1 { get; set; }
        public string intruccion2 { get; set; }
        public string intruccion3 { get; set; }
        public string specs { get; set; }
        public string medida { get; set; }
        public string MedidaNumerador { get; set; }
        public string referencia { get; set; }
        public string tolerancia1 { get; set;}
        public string tolerancia2 { get; set; }

    }
}
