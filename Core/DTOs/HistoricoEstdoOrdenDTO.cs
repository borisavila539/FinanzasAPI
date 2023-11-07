using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class HistoricoEstdoOrdenDTO
    {
        public int ID { get; set; }
        public string usuario { get; set; }
        public string Modulo { get; set; }
        public Boolean Estado { get; set; }
        public DateTime Fecha { get; set; }


    }
}
