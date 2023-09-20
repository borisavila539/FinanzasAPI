using Microsoft.AspNetCore.Mvc;
using Core.Interfaces;
using Core.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FinanzasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditelasController: ControllerBase
    {
        private readonly IAudiTelasRepository _audiTelasRepository;
        private readonly IAX _aX;

        public AuditelasController(IAudiTelasRepository audiTelasRepository,  IAX aX)
        {
            _audiTelasRepository = audiTelasRepository;
            _aX = aX;
        }
        [HttpGet("EnvioAX/{id}")]
        public string GetEnvioAX(int id)
        {
            var resp =  _aX.InsertDefectos(id);
           
            return resp;
        }

        [HttpGet("{RollID}/{ApVendRoll}/{page}/{size}")]
        public async Task<ActionResult<IEnumerable<ObtenerRollosAuditarDTO>>> getRollosAuditar(string RollID, string ApVendRoll, int page, int size)
        {
            var resp = await _audiTelasRepository.GetRollosAuditar(RollID,ApVendRoll,page,size);
            return resp;
        }
        [HttpPost("DatosRollosInsert")]
        public async Task<ActionResult<IEnumerable<DatosRollosInsertDTOs>>> getDatosRollosInsert(List<DatosRollosInsertDTOs> datos)
        {
            var resp = await _audiTelasRepository.postDatosRollos(datos);
            return resp;
        }
        [HttpGet("DatosDefectosTelas/{idrollo}")]
        public async Task<ActionResult<IEnumerable<ObtenerDatosDefectosDTO>>> getDatosDefectos(int idrollo)
        {
            var resp = await _audiTelasRepository.GetDatosDefectos(idrollo);
            return resp;
        }

        [HttpPost("ActualizarRollos")]
        public async Task<ActionResult<IEnumerable<ActualizarRollosDTO>>> getActualizarRollos(List<ActualizarRollosDTO> datos)
        {
            var resp = await _audiTelasRepository.postActualizarRollos(datos);
            return resp;
        }
        [HttpGet("{Id}")]
        public async Task<ActionResult<IEnumerable<ObtenerDetalleRolloDTO>>> getObtenerDetalleRollo(int Id)
        {
            var resp = await _audiTelasRepository.getObtenerDetalleRollo(Id);
            return resp;
        }
        [HttpPost("InsertarAnchoYardas")]
        public async Task<ActionResult<IEnumerable<InsertarAnchoYardasDTO>>> getInsertAnchoYardas(List<InsertarAnchoYardasDTO> datos)
        {
            var resp = await _audiTelasRepository.postInsertarAnchoYardas(datos);
            return resp;
        }
        [HttpGet("DetalleRolloYardas/{Id_Pieza}/{Numero_Rollo_Proveedor}")]
        public async Task<ActionResult<IEnumerable<InsertarAnchoYardasDTO>>> getDetalleRolloYardas(string Id_Pieza, string Numero_Rollo_Proveedor)
        {
            var resp = await _audiTelasRepository.getDatosRollo(Id_Pieza, Numero_Rollo_Proveedor);
            return resp;
        }


    }
}
