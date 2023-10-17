using Core.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IFacturasProveedorRepository
    {
        public Task<string> enviarRetencion(string dataAreaId, string fecha, string correosRecibido, string copiaCorreos);
    }
}
