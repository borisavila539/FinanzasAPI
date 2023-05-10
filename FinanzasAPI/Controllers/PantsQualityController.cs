using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using Core.DTOs;

namespace FinanzasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PantsQualityController : ControllerBase
    {
        private readonly IPantsQualityRepository _pantsQuality;

        public PantsQualityController(IPantsQualityRepository pantsQuality)
        {
            _pantsQuality = pantsQuality;
        }
        [HttpGet("OrdenesIniciadas/{page}/{size}/{filtro}")]
        public async Task<ActionResult<IEnumerable<OrdenesInicialdasDTO>>> GetOrdenesIniciales(int page, int size, string filtro)
        {
            var resp = await _pantsQuality.GetOrdenesInicialdas(page,size,filtro);
            return Ok(resp);
        }

        [HttpGet("orden/{prodmasterid}/{prodmasterrefid}/{itemid}/{estado}")]
        public async Task<ActionResult<IEnumerable<MaestroOrdenes>>> getOrden(string prodmasterrefid, string prodmasterid, string itemid, int estado)
        {
            var resp = await  _pantsQuality.GetOrdenInicialda(prodmasterrefid, prodmasterid, itemid, estado);
            return Ok(resp);
        }

        [HttpGet("tallas/{itemid}")]
        public async Task<ActionResult<IEnumerable<ItemTallasDTOS>>> GetTallasItem(string itemid)
        {
            var resp = await _pantsQuality.GetItemTallas(itemid);
            return Ok(resp);
        }

        [HttpGet("usuario/{user}/{pass}")]
        public async Task<ActionResult<IEnumerable<usuariosDTOs>>> GetUsuario(string user, string pass)
        {
            var resp = await _pantsQuality.GetUsuarios(user, pass);
            return Ok(resp);
        }

        [HttpGet("medidas")]
        public async Task<ActionResult<IEnumerable<MedidasDTOs>>> GetMedidas()
        {
            var resp = await _pantsQuality.GetMedidas();
            return Ok(resp);
        }

        [HttpGet("CambiarEstadoOrden/{id}/{userid}/{estado}")]
        public async Task<ActionResult<IEnumerable<MaestroOrdenes>>> GetPostedMaestroOrdenes(int id, int userid, int estado)
        {
            var resp = await _pantsQuality.GetPostedMaestroOrdenes(id, userid,estado);
            return Ok(resp);
        }

        [HttpGet("DatosMedida/{prodmasterid}/{talla}/{lavado}")]
        public async Task<ActionResult<IEnumerable<DatosMedidasDtos>>> GetDatosMedidas(string prodmasterid, string talla, int lavado)
        {
            var resp = await _pantsQuality.getDatosMedidas(prodmasterid, talla, lavado);
            return Ok(resp);
        }
        [HttpPost("medidasInsert")]
        public  async Task<ActionResult<IEnumerable<MedidasInsertDTOs>>> postMedidasInsert(List<MedidasInsertDTOs> datos)
        {
            var resp = await  _pantsQuality.postMedidasCalidad(datos);
            return resp;
        }


    }
}