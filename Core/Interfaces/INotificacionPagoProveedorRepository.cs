using Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface INotificacionPagoProveedorRepository
    {
        public Task<List<NotificacionPagoDTO>> getNotificacionPago(string numerolote, int enviar, string empresa,string correoCopia);
    }
}
