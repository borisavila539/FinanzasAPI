using Microsoft.AspNetCore.Mvc;
using Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using Core.DTOs;

namespace FinanzasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificacionpagoProveedorController : ControllerBase
    {
        private readonly INotificacionPagoProveedorRepository _notificacionPagoProveedorRepository;
        public NotificacionpagoProveedorController(INotificacionPagoProveedorRepository notificacionPagoProveedorRepository)
        {
            _notificacionPagoProveedorRepository = notificacionPagoProveedorRepository;
        }

        [HttpGet("{numerolote}/{enviar}/{empresa}/{correoCopia}")]
        public async Task<ActionResult<IEnumerable<NotificacionPagoDTO>>> GetNotificacionpagoProveedores(string numerolote, int enviar, string empresa, string correoCopia)
        {
            var resp = await _notificacionPagoProveedorRepository.getNotificacionPago(numerolote, enviar,empresa,correoCopia);
            return Ok(resp);
        }
    }
}
