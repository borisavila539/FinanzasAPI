using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using Core.DTOs;
using Microsoft.AspNetCore.Http;
using System.IO;



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

        [HttpGet("tallas/{itemid}/{prodmasterrefid}")]
        public async Task<ActionResult<IEnumerable<ItemTallasDTOS>>> GetTallasItem(string itemid, string prodmasterrefid)
        {
            var resp = await _pantsQuality.GetItemTallas(itemid, prodmasterrefid);
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

        [HttpPost("comentarioInsert")]
        public async Task<ActionResult<IEnumerable<ComentarioDTO>>> postComentario(List<ComentarioDTO> comentarios)
        {
            var resp = await _pantsQuality.postComentario(comentarios);
            return resp;
        }

        [HttpGet("comentarios/{masterId}")]
        public async Task<ActionResult<IEnumerable<ComentariosDTO>>> getComentarios(int masterId)
        {
            var resp = await _pantsQuality.getComentarios(masterId);
            return Ok(resp);
        }

        [HttpGet("modulosCalidad")]
        public async Task<ActionResult<IEnumerable<ModulosCalidadDTO>>> getModulosCalidad()
        {
            var resp = await _pantsQuality.GetModulosCalidad();
            return resp;

        }
        [HttpGet("HistoricoEstadoOrden/{id}")]
        public async Task<ActionResult<IEnumerable<ComentariosDTO>>> GetHistoricoEstdoOrden(int id)
        {
            var resp = await _pantsQuality.GetHistoricoEstdoOrden(id);
            return Ok(resp);
        }

        [HttpGet("DatosExcel/{prodmasterid}/{itemid}")]
        public Task<string> GetDatosExcel(string prodmasterid, string itemid)
        {
             var resp = _pantsQuality.getDatosExcel(prodmasterid,itemid);
            return resp;
        }

        [HttpPost("subirarchivo")]
        public async Task<List<ResponseDTO>> postExcelCalidad(IFormCollection archivo)
        {
            var lista = new List<ResponseDTO>();
            
            try
            {
                if (Request.Form.Files.Count > 0)
                {
                    string uploadFolerPath = @"\\10.100.0.41\\Auditoria";

                    foreach (var file in Request.Form.Files)
                    {
                        if (!Directory.Exists(uploadFolerPath))
                        {
                            Directory.CreateDirectory(uploadFolerPath);
                        }

                        string fileName = Path.GetFileNameWithoutExtension(file.FileName);


                        var filePath = Path.Combine(uploadFolerPath, file.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            ResponseDTO response = new ResponseDTO();
                            string ruta = uploadFolerPath + "\\" + fileName + ".xlsx";
                            
                            await file.CopyToAsync(stream);

                            
                            //tomar lectura del archivo de excel
                            var resp = await _pantsQuality.GetOrdenesInicialdas(0, 1, fileName);
                            var resp2 = await _pantsQuality.GetOrdenInicialda(resp[0].PRODMASTERREFID, resp[0].PRODMASTERID, resp[0].ITEMID,-1);


                            if (resp.Count > 0)
                            {
                                var datos = await this.GetDatosExcel(fileName, resp[0].ITEMID);
                                if (datos.Substring(0,2) == "no")
                                {
                                    response.response = fileName + ": Error al leer el archivo - " + datos.Substring(3,100);
                                }
                                else
                                {
                                    response.response = fileName + ": Carga exitosa";
                                }
                            }
                            else
                            {
                                response.response = fileName + ": Orden no iniciada";

                            }
                            lista.Add(response);


                        }

                    }                             
                }
            }
            catch(IOException err)
            {
                return lista;
            }
            return lista;
        }



    }
}