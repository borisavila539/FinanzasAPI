using System.Collections.Generic;
using System.Threading.Tasks;
using Core.DTOs;
using Core.Models;

namespace Core.Interfaces
{
    public interface IPantsQualityRepository
    {
        public Task<List<OrdenesInicialdasDTO>> GetOrdenesInicialdas(int page, int size, string filtro);
        public Task<List<MaestroOrdenes>> GetOrdenInicialda(string prodmasterrefid, string prodmasterid, string itemid, int estado);
        public Task<List<ItemTallasDTOS>> GetItemTallas(string itemid);
        public Task<List<usuariosDTOs>> GetUsuarios(string user, string pass);
        public Task<List<MaestroOrdenes>> GetPostedMaestroOrdenes(int id, int userid, int estado);
        public Task<List<MedidasDTOs>> GetMedidas();
        public Task<List<MedidasInsertDTOs>> postMedidasCalidad( List<MedidasInsertDTOs> datos);
        public Task<List<DatosMedidasDtos>> getDatosMedidas(string prodmasterid, string talla, int lavado);
    }
}
