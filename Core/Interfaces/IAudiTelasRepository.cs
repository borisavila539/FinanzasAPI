using Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IAudiTelasRepository
    {
        public Task<List<ObtenerRollosAuditarDTO>> GetRollosAuditar(string RollID, string ApVendRoll,string importacion,string tela, int page, int size);
        public Task<List<DatosRollosInsertDTOs>> postDatosRollos(List<DatosRollosInsertDTOs> datos);
        public Task<List<ObtenerDatosDefectosDTO>> GetDatosDefectos(int idRollo);
        public Task<List<ActualizarRollosDTO>> postActualizarRollos(List<ActualizarRollosDTO> datos);
        public Task<List<ObtenerDetalleRolloDTO>> getObtenerDetalleRollo(int Id); 
        public Task<List<InsertarAnchoYardasDTO>> postInsertarAnchoYardas(List<InsertarAnchoYardasDTO> datos);
        public Task<List<InsertarAnchoYardasDTO>> getDatosRollo(string Id_Pieza, string Numero_Rollo_Proveedor);
        public Task<List<PruebaCalidadDTO>> postPruebasCalidad(List<PruebaCalidadDTO> datos);
        public Task<List<PruebaCalidadDTO>> getPruebasCalidad(int id_rollo);
        public Task<List<AnchoRolloDTO>> setAnchoRollo(string Rollid, decimal Width);
        public Task<List<RollosImporteLoteDTO>> getRollosImporteLote(string Importacion,string Lote, string tela);
        public Task<List<PruebaCalidadLoteDTO>> postPruebaCalidadLote(List<PruebaCalidadLoteDTO> datos);

    }
}
