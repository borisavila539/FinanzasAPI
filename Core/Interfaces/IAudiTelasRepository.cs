using Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IAudiTelasRepository
    {
        public Task<List<ObtenerRollosAuditarDTO>> GetRollosAuditar(string RollID, string ApVendRoll, int page, int size);

    }
}
