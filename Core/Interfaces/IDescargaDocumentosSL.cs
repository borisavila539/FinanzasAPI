using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IDescargaDocumentosSL
    {
        public Task<string> DescargarDocumentos();
    }
}
