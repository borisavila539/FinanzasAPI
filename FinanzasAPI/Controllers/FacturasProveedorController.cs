using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinanzasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacturasProveedorController : ControllerBase
    {
        private readonly IFacturasProveedorRepository _facturasProveedorRepository;
        public FacturasProveedorController(IFacturasProveedorRepository facturasProveedorRepository)
        {
            _facturasProveedorRepository = facturasProveedorRepository;
        }

        [HttpGet("{dataAreaId}/{fecha}/{correosRecibido}/{copiaCorreos}")]
        public async Task<ActionResult<string>> EnviarRetencion(string dataAreaId, string fecha, string correosRecibido, string copiaCorreos)
        {
            var resp = await _facturasProveedorRepository.enviarRetencion(dataAreaId, fecha, correosRecibido, copiaCorreos);
            return Ok(resp);
        }
    }
}
