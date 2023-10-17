using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities
{
    public class Mes
    {
        public string nombreMes(int num)
        {
            string nombre = "";
            switch (num)
            {
                case 1: nombre = "ENERO"; break;
                case 2: nombre = "FEBRERO"; break;
                case 3: nombre = "MARZO"; break;
                case 4: nombre = "ABRIL"; break;
                case 5: nombre = "MAYO"; break;
                case 6: nombre = "JUNIO"; break;
                case 7: nombre = "JULIO"; break;
                case 8: nombre = "AGOSTO"; break;
                case 9: nombre = "SEPTIEMBRE"; break;
                case 10: nombre = "OCTUBRE"; break;
                case 11: nombre = "NOVIEMBRE"; break;
                case 12: nombre = "DICIEMBRE"; break;
            }
            return nombre;
        }
    }
}
