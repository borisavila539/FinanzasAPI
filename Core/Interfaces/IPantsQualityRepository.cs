using System.Collections.Generic;
using System.Threading.Tasks;
using Core.DTOs;
using Core.Models;
using Microsoft.AspNetCore.Http;

namespace Core.Interfaces
{
    public interface IPantsQualityRepository
    {
        public Task<List<OrdenesInicialdasDTO>> GetOrdenesInicialdas(int page, int size, string filtro);
        public Task<List<MaestroOrdenes>> GetOrdenInicialda(string prodmasterrefid, string prodmasterid, string itemid, int estado);
        public Task<List<ItemTallasDTOS>> GetItemTallas(string itemid, string prodmasterrefid);
        public Task<List<usuariosDTOs>> GetUsuarios(string user, string pass);
        public Task<List<MaestroOrdenes>> GetPostedMaestroOrdenes(int id, int userid, int estado);
        public Task<List<MedidasDTOs>> GetMedidas();
        public Task<List<MedidasInsertDTOs>> postMedidasCalidad( List<MedidasInsertDTOs> datos);
        public Task<List<DatosMedidasDtos>> getDatosMedidas(string prodmasterid, string talla, int lavado);
        public Task<List<ComentarioDTO>> postComentario(List<ComentarioDTO> comentario);
        public Task<List<ComentariosDTO>> getComentarios(int masterId);
        public Task<List<ModulosCalidadDTO>> GetModulosCalidad();
        public Task<List<HistoricoEstdoOrdenDTO>> GetHistoricoEstdoOrden(int masterId);
        public Task<string> getDatosExcel(string prodmasterid,string itemid);
        //public Task<string> postExcelCalidad(IFormCollection archivo);
    }
}
