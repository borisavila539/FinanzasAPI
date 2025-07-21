using Core.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IFacturasProveedorRepository
    {
        public Task<string> enviarRetencion(string dataAreaId, string fecha, string correosRecibido, string copiaCorreos);
        public Task<string> enviarCorreo(string correoDestino, string asunto, string textBody, List<Archivos> archivos = null, byte[] imagen = null, List<string> copiasCorreo = null);
    }
}
