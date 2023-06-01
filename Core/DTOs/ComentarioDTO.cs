using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class ComentarioDTO
    {
        public int MasterID { get; set; }
        public int usuario { get; set; }
        public string comentario { get; set; }
        public DateTime? fecha { get; set; }
    }
}
